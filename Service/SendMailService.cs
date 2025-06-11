using AutoMapper;
using Contracts;
using EmailService;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.External;
using System.Text.Json;

namespace Service
{
    public class SendMailService : ISendMailService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IEmailSender _emailSender;
        private readonly HttpClient _httpClient;

        public SendMailService(
            IRepositoryManager repository,
            ILoggerManager logger,
            IEmailSender emailSender,
            HttpClient httpClient)
        {
            _repository = repository;
            _logger = logger;
            _emailSender = emailSender;
            _httpClient = httpClient;
        }

        public async Task ProcessNewOrdersAsync()
        {
            try
            {
                // Lấy mã đơn hàng từ Google APIs ngày hôm nay
                var todayOrderCodes = await GetTodayOrderCodesFromGoogleApisAsync();

                foreach (var orderCode in todayOrderCodes)
                {
                    if (string.IsNullOrEmpty(orderCode))
                        continue;

                    // Kiểm tra xem đã có trong SendMail chưa
                    bool exists = await _repository.SendMail.ExistsAsync(orderCode);
                    if (exists)
                        continue;

                    // Tạo record SendMail với status Pending
                    var sendMail = new SendMail
                    {
                        OrderCode = orderCode,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    _repository.SendMail.CreateSendMail(sendMail);
                    _logger.LogInfo($"Added new order {orderCode} to SendMail queue");
                }

                await _repository.SaveAsync();

                // Xử lý các SendMail pending
                await ProcessPendingSendMailsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ProcessNewOrdersAsync: {ex.Message}");
                throw;
            }
        }

        private async Task<List<string>> GetTodayOrderCodesFromGoogleApisAsync()
        {
            var today = DateTime.Today;
            var orderCodes = new HashSet<string>();

            try
            {
                // 1. Lấy từ Google Calendar ngày hôm nay
                var calendarOrderCodes = await GetTodayOrderCodesFromCalendarAsync(today);
                foreach (var code in calendarOrderCodes)
                {
                    orderCodes.Add(code);
                }

                // 2. Lấy từ Google Sheets (sheet đầu tiên) ngày hôm nay
                var sheetOrderCodes = await GetTodayOrderCodesFromSheetAsync(today);
                foreach (var code in sheetOrderCodes)
                {
                    orderCodes.Add(code);
                }

                _logger.LogInfo($"Found {orderCodes.Count} unique order codes for today");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting today's order codes: {ex.Message}");
            }

            return orderCodes.ToList();
        }

        private async Task<List<string>> GetTodayOrderCodesFromCalendarAsync(DateTime today)
        {
            var orderCodes = new List<string>();

            try
            {
                // Sử dụng CalendarReportService để lấy events từ Google Calendar API
                var userCalendarList = (await _repository.UserCalendar.GetUserCalendarsAsync(false)).ToList();

                // Khởi tạo Google Calendar Service (copy logic từ CalendarReportService)
                var credential = await AuthenticateCalendarAsync();
                var service = new Google.Apis.Calendar.v3.CalendarService(new Google.Apis.Services.BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "SendMail Calendar Check"
                });

                var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "TIMELINE - XƯỞNG", "Tasks", "Sinh nhật", "ATSCADA SOFT",
                    "Holidays in Vietnam", "soft@atpro.com.vn"
                };

                var calendars = service.CalendarList.List().Execute().Items;

                foreach (var cal in calendars)
                {
                    if (excluded.Contains(cal.Summary)) continue;

                    var userCal = userCalendarList.FirstOrDefault(x =>
                        string.Equals(x.Name, cal.Summary, StringComparison.OrdinalIgnoreCase));
                    if (userCal == null) continue;

                    var request = service.Events.List(cal.Id);
                    request.TimeMin = today;
                    request.TimeMax = today.AddDays(1);
                    request.ShowDeleted = false;
                    request.SingleEvents = true;
                    request.MaxResults = 250;

                    var events = await request.ExecuteAsync();

                    foreach (var evt in events.Items)
                    {
                        if (string.IsNullOrEmpty(evt.Summary) || !evt.Summary.StartsWith("DH", StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Extract order code từ summary
                        var match = System.Text.RegularExpressions.Regex.Match(evt.Summary, @"^(DH\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            orderCodes.Add(match.Groups[1].Value.ToUpper());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting calendar events: {ex.Message}");
            }

            return orderCodes;
        }

        private async Task<List<string>> GetTodayOrderCodesFromSheetAsync(DateTime today)
        {
            var orderCodes = new List<string>();

            try
            {
                // Sử dụng Google Sheets API để lấy dữ liệu từ sheet đầu tiên
                var credential = await AuthenticateSheetAsync();
                var sheetsService = new Google.Apis.Sheets.v4.SheetsService(new Google.Apis.Services.BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "SendMail Sheet Check"
                });

                var spreadsheetId = "18zOiaW16z1-cmmDfzOC3_aPd8SJNZj432d3uvw7M8YY";

                // Lấy danh sách sheets
                var spreadsheet = await sheetsService.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
                var firstSheet = spreadsheet.Sheets.FirstOrDefault();

                if (firstSheet == null) return orderCodes;

                string sheetName = firstSheet.Properties.Title;
                var range = $"{sheetName}!A6:G"; // Từ hàng 6 trở đi

                var request = sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
                var response = await request.ExecuteAsync();

                if (response.Values == null) return orderCodes;

                foreach (var row in response.Values)
                {
                    if (row.Count <= 1 || string.IsNullOrWhiteSpace(row[1]?.ToString()))
                        continue;

                    // Kiểm tra xem có OrderCode trong ngày hôm nay không
                    // Giả sử cột G là ngày tạo hoặc cần check logic khác
                    var orderCode = row[1].ToString();

                    // Extract DH code
                    var match = System.Text.RegularExpressions.Regex.Match(orderCode, @"^(DH\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        orderCodes.Add(match.Groups[1].Value.ToUpper());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting sheet data: {ex.Message}");
            }

            return orderCodes;
        }

        private async Task<Google.Apis.Auth.OAuth2.UserCredential> AuthenticateCalendarAsync()
        {
            var googleAuthPath = Path.Combine(Directory.GetCurrentDirectory(), "GoogleAuth");
            using var stream = new FileStream(Path.Combine(googleAuthPath, "credentials.json"), FileMode.Open, FileAccess.Read);

            return await Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
                Google.Apis.Auth.OAuth2.GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { Google.Apis.Calendar.v3.CalendarService.Scope.CalendarReadonly },
                "user",
                CancellationToken.None,
                new Google.Apis.Util.Store.FileDataStore(googleAuthPath, true));
        }

        private async Task<Google.Apis.Auth.OAuth2.UserCredential> AuthenticateSheetAsync()
        {
            var googleAuthPath = Path.Combine(Directory.GetCurrentDirectory(), "GoogleAuth");
            using var stream = new FileStream(Path.Combine(googleAuthPath, "credentials.json"), FileMode.Open, FileAccess.Read);

            return await Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
                Google.Apis.Auth.OAuth2.GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { Google.Apis.Sheets.v4.SheetsService.Scope.SpreadsheetsReadonly },
                "user",
                CancellationToken.None,
                new Google.Apis.Util.Store.FileDataStore(Path.Combine(googleAuthPath, "token.sheets.json"), true));
        }

        private async Task ProcessPendingSendMailsAsync()
        {
            // ✅ CHỈ LẤY SỐ LƯỢNG GIỚI HẠN
            var pendingMails = await _repository.SendMail.GetPendingSendMailsAsync(false);
            var emailsToProcess = pendingMails.Take(3).ToList(); // CHỈ 3 EMAILS MỖI LẦN

            if (!emailsToProcess.Any())
            {
                _logger.LogInfo("📭 No pending emails to process");
                return;
            }

            _logger.LogInfo($"📧 Processing {emailsToProcess.Count} pending emails (limited batch)");

            foreach (var sendMail in emailsToProcess)
            {
                try
                {
                    _logger.LogInfo($"🔄 Processing email for order: {sendMail.OrderCode}");

                    // 1. Gọi API lấy thông tin order
                    var orderInfo = await GetOrderInfoAsync(sendMail.OrderCode);
                    if (orderInfo?.order_info == null)
                    {
                        await UpdateSendMailStatusAsync(sendMail.OrderCode, "Failed", "Cannot get order info");
                        continue;
                    }

                    // 2. Gọi API lấy thông tin account
                    var accountInfo = await GetAccountInfoAsync(orderInfo.order_info.account_code);
                    if (accountInfo?.info == null)
                    {
                        await UpdateSendMailStatusAsync(sendMail.OrderCode, "Failed", "Cannot get account info");
                        continue;
                    }

                    // 3. Lấy email từ info hoặc contacts
                    string email = accountInfo.info.email;
                    if (string.IsNullOrEmpty(email) && accountInfo.contacts?.Any() == true)
                    {
                        email = accountInfo.contacts.FirstOrDefault(c => !string.IsNullOrEmpty(c.email))?.email;
                    }

                    if (string.IsNullOrEmpty(email))
                    {
                        await UpdateSendMailStatusAsync(sendMail.OrderCode, "Failed", "No email found");
                        continue;
                    }

                    // 4. Gửi email (với rate limiting tự động)
                    await SendOrderNotificationEmailAsync(sendMail.OrderCode, email, orderInfo);

                    // 5. Cập nhật status thành Completed
                    await UpdateSendMailStatusAsync(sendMail.OrderCode, "Completed");

                    _logger.LogInfo($"✅ Successfully sent email for order {sendMail.OrderCode} to {email}");

                    // ✅ DELAY GIỮA CÁC EMAIL ĐỂ TRÁNH BURST
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Rate limit"))
                {
                    _logger.LogWarn($"⏸️ Rate limit hit, stopping batch processing. Remaining emails will be processed next cycle.");
                    break; // Dừng batch hiện tại
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Error processing order {sendMail.OrderCode}: {ex.Message}");
                    await UpdateSendMailStatusAsync(sendMail.OrderCode, "Failed", ex.Message);
                }
            }
        }

        public async Task<OrderApiResponse?> GetOrderInfoAsync(string orderCode)
        {
            try
            {
                var apiUrlOrder = $"https://atpro.getflycrm.com/api/v3/orders/{orderCode}";
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-API-KEY", "S4AwIQJXCnwjdsFYNkdmhjZopBubDQ");

                var response = await client.GetAsync(apiUrlOrder);

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrderApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling order API for {orderCode}: {ex.Message}");
                return null;
            }
        }

        public async Task<AccountApiResponse?> GetAccountInfoAsync(string accountCode)
        {
            try
            {
                var apiUrlAccount = $"https://atpro.getflycrm.com/api/v3/account?account_code={accountCode}";
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-API-KEY", "S4AwIQJXCnwjdsFYNkdmhjZopBubDQ");

                var response = await client.GetAsync(apiUrlAccount);

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AccountApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling account API for {accountCode}: {ex.Message}");
                return null;
            }
        }

        public async Task SendOrderNotificationEmailAsync(string orderCode, string email, OrderApiResponse orderInfo)
        {
            var subject = $"Thông báo đơn hàng {orderCode} - Công ty Cổ Phần Giải Pháp Kỹ Thuật Ấn Tượng";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #ffffff; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background-color: #ffffff; padding: 30px; text-align: center; color: white; }}
        .logo {{ max-width: 200px; height: auto; }}
        .header h1 {{ color: white; margin: 15px 0 0 0; font-size: 24px; }}
        .content {{ padding: 30px; text-align: center; background-color: white; }}
        .order-code {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; font-size: 18px; border: 1px solid #e9ecef; }}
        .tracking-section {{ background-color: #063b6a; padding: 25px; border-radius: 10px; text-align: center; margin: 25px 0; color: white; }}
        .tracking-section h3 {{ color: white; margin-top: 0; }}
        .tracking-link {{ display: inline-block; background-color: #28a745; color: white; padding: 15px 40px; text-decoration: none; border-radius: 25px; font-weight: 600; margin-top: 15px; font-size: 16px; }}
        .tracking-link:hover {{ background-color: #218838; }}
        .footer {{ background-color: white; padding: 0; text-align: center; }}
        .footer-logo {{ width: 100%; max-width: none; height: auto; display: block; }}
        
        @media only screen and (max-width: 600px) {{
            body {{ padding: 10px; }}
            .container {{ width: 100%; }}
            .content {{ padding: 20px; }}
            .header {{ padding: 20px; }}
            .tracking-link {{ padding: 12px 30px; font-size: 14px; }}
            .footer-logo {{ width: 100%; }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://atpro.com.vn/wp-content/uploads/2020/10/logo-cong-ty-1024x342-1024x342.png' alt='Ấn Tượng Technology' class='logo'>
        </div>
        
        <div class='content'>
            <h2 style='color: #495057;'>Xin chào {orderInfo.order_info.account_name},</h2>
            <p style='color: #6c757d; line-height: 1.6; font-size: 16px;'>
                Đơn hàng của quý khách đã được chuyển giao sản xuất.
            </p>
            
            <div class='order-code'>
                <strong>Mã đơn hàng: {orderInfo.order_info.order_code}</strong>
            </div>
            
            <div class='tracking-section'>
                <h3>🔍 Theo dõi tiến độ đơn hàng</h3>
                <a href='https://atlink.asia/OrderTracking/find?order={orderInfo.order_info.order_code}' class='tracking-link'>
                    Theo dõi đơn hàng ngay
                </a>
            </div>
            
        </div>
        
        <div class='footer'>
            <img src='https://ci3.googleusercontent.com/mail-sig/AIorK4zTKBUb1t7Fv2EzQaO2VqFc8LL8rFA_DvavtT_BNQVMWsI37u0tnifcTHLyiiy59Gb3ZVCOUe3wLzbS' alt='Footer Image' class='footer-logo'>
        </div>
    </div>
</body>
</html>";

            var message = new Message(new string[] { email }, subject, body, null);
            await _emailSender.SendEmailAsync(message);
            await Task.Delay(TimeSpan.FromSeconds(30)); // 30 giây delay
        }
        public async Task UpdateSendMailStatusAsync(string orderCode, string status, string? errorMessage = null)
        {
            var sendMail = await _repository.SendMail.GetByOrderCodeAsync(orderCode, true);
            if (sendMail != null)
            {
                sendMail.Status = status;
                sendMail.ProcessedAt = DateTime.UtcNow;
                sendMail.ErrorMessage = errorMessage;

                _repository.SendMail.UpdateSendMail(sendMail);
                await _repository.SaveAsync();
            }
        }
    }
}
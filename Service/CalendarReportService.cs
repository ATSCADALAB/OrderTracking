using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Hosting;
using Service.Contracts;
using System.Drawing;
using System.Text.RegularExpressions;
using Shared.DataTransferObjects.CalendarReport;
using AutoMapper;
using Contracts;
using DocumentFormat.OpenXml.Spreadsheet;
using Shared.DataTransferObjects.UserCalendar;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using Shared.DataTransferObjects.Calendar;
using Color = Google.Apis.Sheets.v4.Data.Color;
using Microsoft.Extensions.Caching.Memory;
using Entities.Models;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

namespace Service
{
    internal sealed class CalendarReportService : ICalendarReportService
    {
        #region Private Fields and Constructor

        private readonly string _googleAuthPath;
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly string _spreadsheetId = "18zOiaW16z1-cmmDfzOC3_aPd8SJNZj432d3uvw7M8YY";

        public CalendarReportService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IWebHostEnvironment env)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _googleAuthPath = Path.Combine(env.ContentRootPath, "GoogleAuth");
        }

        #endregion

        #region Google Sheets Sync Methods

        public async Task SyncSheetOrdersAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Khởi tạo Google Sheets Service
                var credential = await AuthenticateSheetAsync();
                var sheetsService = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Sheet Order Sync"
                });

                // Lấy danh sách tất cả các sheet từ Google Sheets
                var spreadsheet = await sheetsService.Spreadsheets.Get(_spreadsheetId).ExecuteAsync(cancellationToken);
                var sheets = spreadsheet.Sheets.ToList();

                if (sheets.Count < 4)
                {
                    return;
                }

                // Bỏ qua ba sheet đầu tiên
                var sheetsToProcess = sheets.Skip(3).ToList();
                if (!sheetsToProcess.Any())
                {
                    return;
                }

                var allSheetOrders = new List<SheetOrder>();
                var batchRanges = new List<string>();
                var formatRanges = new List<string>();

                // Tạo danh sách các phạm vi (ranges) để lấy dữ liệu và định dạng
                foreach (var sheet in sheetsToProcess)
                {
                    string sheetName = sheet.Properties.Title;
                    batchRanges.Add($"{sheetName}!A6:G");
                    formatRanges.Add($"{sheetName}!A6:A1000"); // Giả định tối đa 1000 dòng
                }

                // Batch request để lấy dữ liệu
                var batchRequest = sheetsService.Spreadsheets.Values.BatchGet(_spreadsheetId);
                batchRequest.Ranges = batchRanges;
                var batchResponse = await batchRequest.ExecuteAsync(cancellationToken);

                // Batch request để lấy định dạng màu nền
                var formatRequest = sheetsService.Spreadsheets.Get(_spreadsheetId);
                formatRequest.Ranges = formatRanges;
                formatRequest.Fields = "sheets.data.rowData.values.effectiveFormat.backgroundColor";
                var formatResponse = await formatRequest.ExecuteAsync(cancellationToken);

                for (int sheetIndex = 0; sheetIndex < sheetsToProcess.Count; sheetIndex++)
                {
                    var sheet = sheetsToProcess[sheetIndex];
                    string sheetName = sheet.Properties.Title;
                    var valueRange = batchResponse.ValueRanges[sheetIndex];
                    var rows = valueRange?.Values;
                    if (rows == null || rows.Count == 0)
                    {
                        continue;
                    }

                    var rowFormats = formatResponse.Sheets[sheetIndex].Data[0].RowData;

                    for (int i = 0; i < rows.Count; i++)
                    {
                        var row = rows[i];
                        if (row.Count <= 1 || string.IsNullOrWhiteSpace(row[1]?.ToString()))
                        {
                            continue;
                        }

                        string orderCode = row[1].ToString();
                        if (string.IsNullOrWhiteSpace(orderCode) || orderCode.Length > 50)
                        {
                            continue;
                        }

                        bool exists = await _repository.SheetOrder.ExistsAsync(orderCode, sheetName);
                        if (exists)
                        {
                            continue;
                        }

                        var color = rowFormats[i]?.Values?[0]?.EffectiveFormat?.BackgroundColor;
                        string hexColor = color != null ? ConvertColorToHex(color) : "#FFFFFF";

                        var sheetOrder = new SheetOrder
                        {
                            OrderCode = orderCode.Substring(0, Math.Min(orderCode.Length, 50)),
                            OrderName = row.Count > 1 ? (row[1]?.ToString() ?? "").Substring(0, Math.Min(row[1]?.ToString().Length ?? 0, 255)) : "",
                            Dev1 = row.Count > 2 ? (row[2]?.ToString() ?? "").Substring(0, Math.Min(row[2]?.ToString().Length ?? 0, 100)) : "",
                            QC = row.Count > 3 ? (row[3]?.ToString() ?? "").Substring(0, Math.Min(row[3]?.ToString().Length ?? 0, 100)) : "",
                            Code = row.Count > 4 ? (row[4]?.ToString() ?? "").Substring(0, Math.Min(row[4]?.ToString().Length ?? 0, 100)) : "",
                            Sale = row.Count > 5 ? (row[5]?.ToString() ?? "").Substring(0, Math.Min(row[5]?.ToString().Length ?? 0, 100)) : "",
                            EndDate = row.Count > 6 ? ParseDate(row[6]?.ToString()) : null,
                            Status = MapColorToStatus(hexColor),
                            SheetName = sheetName.Substring(0, Math.Min(sheetName.Length, 50)),
                            CreatedAt = DateTime.UtcNow
                        };
                        _repository.SheetOrder.CreateSheetOrder(sheetOrder);
                        allSheetOrders.Add(sheetOrder);
                    }
                }

                if (allSheetOrders.Any())
                {
                    await _repository.SaveAsync();
                }
                else
                {
                }
            }
            catch (Google.GoogleApiException ex)
            {
                throw new Exception("Không thể truy cập Google Sheets. Vui lòng kiểm tra log để biết thêm chi tiết.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi không xác định khi đồng bộ hóa dữ liệu. Vui lòng kiểm tra log để biết thêm chi tiết.", ex);
            }
        }

        public async Task<SheetOrderDto?> SearchOrderFromSheetAsync(string orderCode)
        {
            try
            {
                // Bước 1: Tìm kiếm trong cơ sở dữ liệu
                var sheetOrder = await _repository.SheetOrder.GetByOrderCodeAsync(orderCode);
                if (sheetOrder != null)
                {
                    return new SheetOrderDto
                    {
                        OrderCode = sheetOrder.OrderCode,
                        OrderName = sheetOrder.OrderName ?? "",
                        Dev1 = sheetOrder.Dev1 ?? "",
                        QC = sheetOrder.QC ?? "",
                        Code = sheetOrder.Code ?? "",
                        Sale = sheetOrder.Sale ?? "",
                        EndDate = sheetOrder.EndDate,
                        Status = sheetOrder.Status ?? "",
                    };
                }

                // Bước 2: Tìm kiếm trong ba sheet đầu tiên của Google Sheets
                var credential = await AuthenticateSheetAsync();
                var sheetsService = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Sheet Order Search"
                });

                var spreadsheet = await sheetsService.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
                var sheets = spreadsheet.Sheets.Take(3).ToList(); // Chỉ lấy ba sheet đầu tiên
                if (!sheets.Any())
                {
                    return null;
                }

                // Sử dụng batch request để lấy dữ liệu từ ba sheet
                var batchRanges = sheets.Select(s => $"{s.Properties.Title}!A6:G").ToList();
                var batchRequest = sheetsService.Spreadsheets.Values.BatchGet(_spreadsheetId);
                batchRequest.Ranges = batchRanges;
                var batchResponse = await batchRequest.ExecuteAsync();

                // Lấy định dạng màu nền cho ba sheet
                var formatRanges = sheets.Select(s => $"{s.Properties.Title}!A6:A1000").ToList(); // Giả định tối đa 1000 dòng
                var formatRequest = sheetsService.Spreadsheets.Get(_spreadsheetId);
                formatRequest.Ranges = formatRanges;
                formatRequest.Fields = "sheets.data.rowData.values.effectiveFormat.backgroundColor";
                var formatResponse = await formatRequest.ExecuteAsync();

                for (int sheetIndex = 0; sheetIndex < sheets.Count; sheetIndex++)
                {
                    var sheet = sheets[sheetIndex];
                    string sheetName = sheet.Properties.Title;
                    var valueRange = batchResponse.ValueRanges[sheetIndex];
                    var rows = valueRange?.Values;
                    if (rows == null || rows.Count == 0)
                    {
                        continue;
                    }

                    var rowFormats = formatResponse.Sheets[sheetIndex].Data[0].RowData;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var row = rows[i];
                        if (row.Count <= 1 || string.IsNullOrWhiteSpace(row[1]?.ToString()))
                            continue;

                        string foundOrderCode = row[1].ToString().ToLower();
                        if (foundOrderCode.Contains(orderCode.ToLower()))
                        {
                            var color = rowFormats[i]?.Values?[0]?.EffectiveFormat?.BackgroundColor;
                            string hexColor = color != null ? ConvertColorToHex(color) : "#FFFFFF";

                            var sheetOrderDto = new SheetOrderDto
                            {
                                OrderCode = orderCode,
                                OrderName = row.Count > 1 ? (row[1]?.ToString() ?? "").Substring(0, Math.Min(row[1]?.ToString().Length ?? 0, 255)) : "",
                                Dev1 = row.Count > 2 ? (row[2]?.ToString() ?? "").Substring(0, Math.Min(row[2]?.ToString().Length ?? 0, 100)) : "",
                                QC = row.Count > 3 ? (row[3]?.ToString() ?? "").Substring(0, Math.Min(row[3]?.ToString().Length ?? 0, 100)) : "",
                                Code = row.Count > 4 ? (row[4]?.ToString() ?? "").Substring(0, Math.Min(row[4]?.ToString().Length ?? 0, 100)) : "",
                                Sale = row.Count > 5 ? (row[5]?.ToString() ?? "").Substring(0, Math.Min(row[5]?.ToString().Length ?? 0, 100)) : "",
                                EndDate = row.Count > 6 ? ParseDate(row[6]?.ToString()) : null,
                                Status = MapColorToStatus(hexColor)
                            };

                            return sheetOrderDto;
                        }
                    }
                }

                return null;
            }
            catch (Google.GoogleApiException ex)
            {
                throw new Exception("Không thể truy cập Google Sheets. Vui lòng kiểm tra log.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi không xác định khi tìm kiếm đơn hàng. Vui lòng kiểm tra log.", ex);
            }
        }

        #endregion

        #region Google Calendar Sync Methods

        public async Task SyncCalendarEventsAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Lấy danh sách calendar users từ database
                var userCalendarList = (await _repository.UserCalendar.GetUserCalendarsAsync(false)).ToList();
                var userCalendarDto = _mapper.Map<IEnumerable<UserCalendarDto>>(userCalendarList);

                // Khởi tạo Google Calendar Service
                UserCredential credential;
                using (var stream = new FileStream(Path.Combine(_googleAuthPath, "credentials.json"), FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { CalendarService.Scope.CalendarReadonly },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(_googleAuthPath, true));
                }

                var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Calendar Event Sync"
                });

                // Danh sách calendar bị loại trừ
                var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "TIMELINE - XƯỞNG", "Tasks", "Sinh nhật", "ATSCADA SOFT",
                    "Holidays in Vietnam", "soft@atpro.com.vn"
                };

                // Xác định ngày giới hạn (23/03/2025 nếu hôm nay là 23/05/2025)
                var endDate = DateTime.UtcNow.AddMonths(-2); // 23/03/2025

                // Lấy danh sách tất cả calendars
                var calendars = service.CalendarList.List().Execute().Items;
                var allCalendarEvents = new List<CalendarEvent>();

                foreach (var cal in calendars)
                {
                    if (excluded.Contains(cal.Summary)) continue;

                    var userCal = userCalendarDto.FirstOrDefault(x =>
                        string.Equals(x.Name, cal.Summary, StringComparison.OrdinalIgnoreCase));
                    if (userCal == null) continue;

                    string pageToken = null;
                    do
                    {
                        var request = service.Events.List(cal.Id);
                        request.ShowDeleted = false;
                        request.SingleEvents = true;
                        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                        request.MaxResults = 250;
                        request.PageToken = pageToken;
                        request.TimeMax = endDate; // Giới hạn sự kiện từ 23/03/2025 trở về trước

                        var response = await request.ExecuteAsync(cancellationToken);
                        var validEvents = response.Items
                            .Where(e => !string.IsNullOrEmpty(e.Summary) &&
                                        e.Summary.StartsWith("DH", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        foreach (var ev in validEvents)
                        {
                            // Kiểm tra xem event đã tồn tại trong database chưa
                            var eventId = ev.Id;
                            var exists = await _repository.CalendarEvent.ExistsAsync(eventId);
                            if (exists) continue;

                            // Parse thông tin từ event với phương thức cải thiện
                            var (orderCode, sale, title) = ParseSummaryComplete(ev.Summary);

                            var calendarEvent = new CalendarEvent
                            {
                                Title = ev.Summary,
                                OrderCode = orderCode,
                                Description = ev.Description,
                                StartDate = ev.Start.DateTime ?? DateTime.Parse(ev.Start.Date),
                                EndDate = (ev.End.DateTime ?? DateTime.Parse(ev.End.Date)).AddDays(-1),
                                UserName = userCal.UserName,
                                CreatedAt = DateTime.UtcNow
                            };

                            _repository.CalendarEvent.CreateCalendarEvent(calendarEvent);
                            allCalendarEvents.Add(calendarEvent);
                        }

                        pageToken = response.NextPageToken;
                    } while (pageToken != null);
                }

                if (allCalendarEvents.Any())
                {
                    await _repository.SaveAsync();
                    _logger.LogInfo($"Đã đồng bộ {allCalendarEvents.Count} events từ Google Calendar đến ngày {endDate:dd/MM/yyyy}");
                }
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.LogError($"Lỗi khi truy cập Google Calendar: {ex.Message}");
                throw new Exception("Không thể truy cập Google Calendar. Vui lòng kiểm tra log để biết thêm chi tiết.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi không xác định: {ex.Message}");
                throw new Exception("Lỗi không xác định khi đồng bộ dữ liệu. Vui lòng kiểm tra log để biết thêm chi tiết.", ex);
            }
        }

        public async Task<OrderEventDto?> SearchEventByOrderCodeAsync(string orderCode, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                throw new ArgumentException("Mã đơn hàng không được để trống.", nameof(orderCode));

            // Bước 1: Tìm kiếm trong database trước
            var calendarEvent = await _repository.CalendarEvent.GetByOrderCodeAsync(orderCode);
            if (calendarEvent != null)
            {
                var userCalendarList1 = (await _repository.UserCalendar.GetUserCalendarsAsync(false)).ToList();
                var userCalendarDto1 = _mapper.Map<IEnumerable<UserCalendarDto>>(userCalendarList1);
                var userCal = userCalendarDto1.FirstOrDefault(x => string.Equals(x.UserName, calendarEvent.UserName, StringComparison.OrdinalIgnoreCase));

                // Sử dụng phương thức cải thiện để parse
                var (parsedOrderCode, sale, title) = ParseSummaryComplete(calendarEvent.Title);
                var qc = ExtractQCFromDescription(calendarEvent.Description ?? "");

                return new OrderEventDto
                {
                    Title = title,
                    OrderCode = parsedOrderCode,
                    StartDate = calendarEvent.StartDate,
                    EndDate = calendarEvent.EndDate,
                    Handler = userCal?.UserName ?? "Không rõ",
                    Timeline = ParseTimelineFromDescription(calendarEvent.Description ?? ""),
                    Sale = sale,
                    QC = qc
                };
            }

            // Bước 2: Tìm kiếm trên Google Calendar nếu không tìm thấy trong database
            var userCalendarList = (await _repository.UserCalendar.GetUserCalendarsAsync(false)).ToList();
            var userCalendarDto = _mapper.Map<IEnumerable<UserCalendarDto>>(userCalendarList);

            UserCredential credential;
            using (var stream = new FileStream(Path.Combine(_googleAuthPath, "credentials.json"), FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { CalendarService.Scope.CalendarReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(_googleAuthPath, true));
            }

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar Order Search"
            });

            var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "TIMELINE - XƯỞNG", "Tasks", "Sinh nhật", "ATSCADA SOFT",
                "Holidays in Vietnam", "soft@atpro.com.vn"
            };

            // Xác định khoảng thời gian tìm kiếm: từ ngày hiện tại của tháng cách 2 tháng đến ngày hiện tại
            var currentDate = DateTime.UtcNow; // 23/05/2025
            var timeMin = currentDate.AddMonths(-2); // 23/03/2025
            var timeMax = currentDate.AddMonths(1); // 23/05/2025

            var calendars = service.CalendarList.List().Execute().Items;
            var allDhEvents = new List<Event>();

            foreach (var cal in calendars)
            {
                if (excluded.Contains(cal.Summary)) continue;

                var userCal = userCalendarDto.FirstOrDefault(x => string.Equals(x.Name, cal.Summary, StringComparison.OrdinalIgnoreCase));
                if (userCal == null) continue;

                string? pageToken = null;
                do
                {
                    var req = service.Events.List(cal.Id);
                    req.ShowDeleted = false;
                    req.SingleEvents = true;
                    req.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                    req.MaxResults = 250;
                    req.PageToken = pageToken;
                    req.TimeMin = timeMin; // Từ 23/03/2025
                    req.TimeMax = timeMax; // Đến 23/05/2025

                    var response = await req.ExecuteAsync(token); // Sử dụng CancellationToken
                    var validEvents = response.Items
                        .Where(e => !string.IsNullOrEmpty(e.Summary) &&
                                    e.Summary.StartsWith("DH", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    allDhEvents.AddRange(validEvents);
                    pageToken = response.NextPageToken;
                } while (pageToken != null);
            }

            var matched = allDhEvents
                .Where(e => (e.Summary != null && e.Summary.Contains(orderCode, StringComparison.OrdinalIgnoreCase)) ||
                            (e.Description != null && e.Description.Contains(orderCode, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(e => e.Start.DateTime ?? DateTime.MinValue)
                .FirstOrDefault();

            if (matched == null)
                return null;

            var dto = await MapEventToOrderDto(matched);
            return dto;
        }

        #endregion

        #region Unified Search Methods

        public async Task<UnifiedOrderDto?> SearchOrderFromAllSourcesAsync(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                throw new ArgumentException("Mã đơn hàng không được để trống.", nameof(orderCode));

            var credential = await AuthenticateSheetAsync();
            var sheetService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Sheets Order Search"
            });

            var sheetResult = await SearchOrderFromSheetAsync(orderCode);
            var calendarResult = await SearchEventByOrderCodeAsync(orderCode, CancellationToken.None);

            if (sheetResult == null && calendarResult == null)
                return null;

            var unifiedDto = new UnifiedOrderDto { OrderCode = orderCode };

            if (sheetResult != null && calendarResult != null)
            {
                unifiedDto.Source = "Sheets";
                unifiedDto.Title = sheetResult.OrderName;
                unifiedDto.EndDate = sheetResult.EndDate;
                unifiedDto.Handler = calendarResult.Handler;
                unifiedDto.Status = sheetResult.Status;
                unifiedDto.Dev1 = sheetResult.Dev1;
                unifiedDto.QC = sheetResult.QC;
                unifiedDto.Code = sheetResult.Code;
                unifiedDto.Sale = sheetResult.Sale;
            }
            else if (calendarResult != null)
            {
                unifiedDto.Source = "Calendar";
                unifiedDto.Title = calendarResult.Title;
                unifiedDto.StartDate = calendarResult.StartDate;
                unifiedDto.EndDate = calendarResult.EndDate;
                unifiedDto.Handler = calendarResult.Handler;
                unifiedDto.Timeline = calendarResult.Timeline;
                unifiedDto.QC = calendarResult.QC;
                unifiedDto.Sale = calendarResult.Sale;
            }
            else
            {
                unifiedDto.Source = "Sheets";
                unifiedDto.Title = sheetResult.OrderName;
                unifiedDto.EndDate = sheetResult.EndDate;
                unifiedDto.Handler = sheetResult.Code;
                unifiedDto.Status = sheetResult.Status;
                unifiedDto.Dev1 = sheetResult.Dev1;
                unifiedDto.QC = sheetResult.QC;
                unifiedDto.Code = sheetResult.Code;
                unifiedDto.Sale = sheetResult.Sale;
            }
            return unifiedDto;
        }

        #endregion

        #region KPI and Report Generation Methods

        public async Task<IEnumerable<CalendarUserKpiDto>> GetUserKpiReportAsync(DateTime startDate, DateTime endDate)
        {
            // Lấy KPI configuration
            var kpiConfig = await _repository.KpiConfiguration.GetActiveConfigurationAsync(false);
            if (kpiConfig == null)
                throw new Exception("No active KPI configuration found");

            var userCalendarList = (await _repository.UserCalendar.GetUserCalendarsAsync(false)).ToList();
            var userCalendarDto = _mapper.Map<IEnumerable<UserCalendarDto>>(userCalendarList);

            var result = new List<CalendarUserKpiDto>();
            UserCredential credential;
            using (var stream = new FileStream(Path.Combine(_googleAuthPath, "credentials.json"), FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { CalendarService.Scope.CalendarReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(_googleAuthPath, true));
            }

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar KPI Report"
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

                var userCal = userCalendarDto.FirstOrDefault(x => string.Equals(x.Name, cal.Summary, StringComparison.OrdinalIgnoreCase));
                if (userCal == null) continue;

                string userName = userCal.UserName ?? cal.Summary;

                var req = service.Events.List(cal.Id);
                req.TimeMin = startDate;
                req.TimeMax = endDate;
                req.ShowDeleted = false;
                req.SingleEvents = true;
                req.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                req.MaxResults = 500;

                var events = req.Execute().Items
                    .Where(e => !string.IsNullOrEmpty(e.Summary))
                    .ToList();

                var orders = new List<CalendarReport>();
                decimal totalPenalty = 0; // Tổng penalty cho user này

                foreach (var ev in events)
                {
                    var desc = Regex.Replace(ev.Description ?? "", "<.*?>", "").Trim();
                    DateTime evStart = ev.Start.DateTime ?? DateTime.Parse(ev.Start.Date);
                    DateTime evEnd = (ev.End.DateTime ?? DateTime.Parse(ev.End.Date)).AddDays(-1);

                    var lastTimelineDate = GetLastTimelineDate(desc);
                    if (lastTimelineDate == null) continue;

                    var qcDate = lastTimelineDate.Value;
                    int delta = (qcDate - evEnd).Days;

                    // SỬ DỤNG KPI CONFIG THAY VÌ HARDCODE
                    int stars = CalculateStarsFromConfig(delta, kpiConfig);
                    int days = (qcDate - evStart).Days;

                    // THÊM LOGIC ĐỌC PB TỪ DESCRIPTION
                    decimal eventPenalty = ExtractPenaltyFromDescription(ev.Description ?? "", kpiConfig);
                    totalPenalty += eventPenalty;

                    orders.Add(new CalendarReport
                    {
                        Title = ev.Summary,
                        Start = evStart,
                        End = evEnd,
                        EventDays = days,
                        Stars = stars,
                        QCDay = qcDate
                    });
                }

                if (!orders.Any()) continue;

                double avgStars = orders.Average(o => o.Stars);

                // SỬ DỤNG KPI CONFIG ĐỂ PHÂN LOẠI ĐƠN HÀNG
                var orderDays = orders.Select(o => o.EventDays).ToList();
                var (small, medium, large) = CategorizeOrdersFromConfig(orderDays, kpiConfig);

                // SỬ DỤNG KPI CONFIG ĐỂ TÍNH HSSL
                double hssl = CalculateHSSLFromConfig(small, medium, large, kpiConfig);

                // SỬ DỤNG PENALTY THỰC TẾ TỪ PB THAY VÌ MẶC ĐỊNH
                double penalty = (double)totalPenalty; // Sử dụng tổng penalty từ tất cả events
                double reward = CalculateRewardFromConfig((decimal)avgStars, (decimal)hssl, (decimal)penalty, kpiConfig);

                result.Add(new CalendarUserKpiDto
                {
                    UserName = userName,
                    SmallOrders = small,
                    MediumOrders = medium,
                    LargeOrders = large,
                    AverageStars = Math.Round(avgStars, 2),
                    RewardOrPenalty = Math.Round(reward, 2)
                });
            }

            return result;
        }
        private DateTime? GetLastTimelineDate(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            try
            {
                // Parse timeline từ description
                var timelineSteps = ParseTimelineFromDescription(description);

                if (!timelineSteps.Any())
                    return null;

                // Lấy ngày cuối cùng trong timeline
                return timelineSteps.OrderByDescending(t => t.Date).First().Date;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<byte[]> GenerateReportAsync(DateTime startDate, DateTime endDate)
        {
            // Lấy KPI configuration
            var kpiConfig = await _repository.KpiConfiguration.GetActiveConfigurationAsync(false);
            if (kpiConfig == null)
                throw new Exception("No active KPI configuration found");

            var userCalendar = (await _repository.UserCalendar.GetUserCalendarsAsync(false)).ToList();
            var userCalendarDto = _mapper.Map<IEnumerable<UserCalendarDto>>(userCalendar);

            UserCredential credential;
            using (var stream = new FileStream(Path.Combine(_googleAuthPath, "credentials.json"), FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { CalendarService.Scope.CalendarReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(_googleAuthPath, true));
            }

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar API Export"
            });

            var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "TIMELINE - XƯỞNG", "Tasks", "Sinh nhật", "ATSCADA SOFT",
        "Holidays in Vietnam", "soft@atpro.com.vn"
    };

            var workbook = new XLWorkbook();
            var calendars = service.CalendarList.List().Execute().Items;

            foreach (var cal in calendars)
            {
                if (excluded.Contains(cal.Summary)) continue;

                var req = service.Events.List(cal.Id);
                req.TimeMin = startDate;
                req.TimeMax = endDate.AddDays(1);
                req.ShowDeleted = false;
                req.SingleEvents = true;
                req.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                req.MaxResults = 500;

                var events = req.Execute().Items
                    .Where(e => !string.IsNullOrEmpty(e.Summary))
                    .ToList();
                if (!events.Any()) continue;

                var orders = new List<CalendarReport>();
                decimal totalPenalty = 0; // Tổng penalty cho calendar này

                foreach (var ev in events)
                {
                    var desc = Regex.Replace(ev.Description ?? "", "<.*?>", "").Trim();
                    DateTime evStart = ev.Start.DateTime ?? DateTime.Parse(ev.Start.Date);
                    DateTime evEnd = (ev.End.DateTime ?? DateTime.Parse(ev.End.Date)).AddDays(-1);

                    var lastTimelineDate = GetLastTimelineDate(desc);
                    if (lastTimelineDate == null) continue;

                    var qcDate = lastTimelineDate.Value;
                    int delta = (qcDate - evEnd).Days;

                    // SỬ DỤNG KPI CONFIG
                    int stars = CalculateStarsFromConfig(delta, kpiConfig);
                    int days = (qcDate - evStart).Days;

                    // THÊM LOGIC ĐỌC PB TỪ DESCRIPTION
                    decimal eventPenalty = ExtractPenaltyFromDescription(ev.Description ?? "", kpiConfig);
                    totalPenalty += eventPenalty;

                    orders.Add(new CalendarReport
                    {
                        Title = ev.Summary,
                        Start = evStart,
                        End = evEnd,
                        EventDays = days,
                        Stars = stars,
                        QCDay = qcDate
                    });
                }

                if (!orders.Any()) continue;

                double avgStars = orders.Average(o => o.Stars);

                // SỬ DỤNG KPI CONFIG
                var orderDays = orders.Select(o => o.EventDays).ToList();
                var (sln, slv, sll) = CategorizeOrdersFromConfig(orderDays, kpiConfig);
                double hssl = CalculateHSSLFromConfig(sln, slv, sll, kpiConfig);

                // SỬ DỤNG PENALTY THỰC TẾ TỪ PB THAY VÌ MẶC ĐỊNH
                double penalty = (double)totalPenalty; // Sử dụng tổng penalty từ tất cả events
                double reward = CalculateRewardFromConfig((decimal)avgStars, (decimal)hssl, (decimal)penalty, kpiConfig);

                var userCal = userCalendarDto.FirstOrDefault(x => string.Equals(x.Name, cal.Summary, StringComparison.OrdinalIgnoreCase));
                if (userCal == null) continue;

                string userName = userCal.UserName ?? cal.Summary;
                var sheetName = userName.Length > 31 ? userName[..31] : userName;
                var sheet = workbook.Worksheets.Add(sheetName);

                sheet.Cell(1, 1).Value = "Average Stars";
                sheet.Cell(1, 2).Value = Math.Round(avgStars, 2);
                sheet.Cell(2, 1).Value = $"Small (<{kpiConfig.LightOrder_MaxDays} days)";
                sheet.Cell(2, 2).Value = sln;
                sheet.Cell(3, 1).Value = $"Medium ({kpiConfig.MediumOrder_MinDays}-{kpiConfig.MediumOrder_MaxDays} days)";
                sheet.Cell(3, 2).Value = slv;
                sheet.Cell(4, 1).Value = $"Large (>={kpiConfig.HeavyOrder_MinDays} days)";
                sheet.Cell(4, 2).Value = sll;
                sheet.Cell(5, 1).Value = "HSSL";
                sheet.Cell(5, 2).Value = Math.Round(hssl, 2);
                sheet.Cell(6, 1).Value = "Total Penalty (PB)";
                sheet.Cell(6, 2).Value = Math.Round(penalty, 2);
                sheet.Cell(7, 1).Value = "Bonus/Penalty";
                sheet.Cell(7, 2).Value = Math.Round(reward, 2);

                // Tiếp tục với việc format sheet...
                sheet.Cell(9, 1).Value = "Title";
                sheet.Cell(9, 2).Value = "Start";
                sheet.Cell(9, 3).Value = "End";
                sheet.Cell(9, 4).Value = "QCDay";
                sheet.Cell(9, 5).Value = "Days";
                sheet.Cell(9, 6).Value = "Stars";

                int row = 10;
                foreach (var o in orders)
                {
                    sheet.Cell(row, 1).Value = o.Title;
                    sheet.Cell(row, 2).Value = o.Start.ToString("dd/MM/yyyy");
                    sheet.Cell(row, 3).Value = o.End.ToString("dd/MM/yyyy");
                    sheet.Cell(row, 4).Value = o.QCDay.ToString("dd/MM/yyyy");
                    sheet.Cell(row, 5).Value = o.EventDays + 1;
                    sheet.Cell(row, 6).Value = o.Stars;
                    row++;
                }

                // Format Excel như cũ...
                sheet.Columns().AdjustToContents();
                var summaryRange = sheet.Range("A1:B7"); // Thay đổi từ B6 thành B7 để bao gồm Total Penalty
                summaryRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                summaryRange.Style.Font.Bold = true;
                summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                var headerRange = sheet.Range("A9:F9"); // Thay đổi từ A8 thành A9
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                var lastRow = sheet.LastRowUsed().RowNumber();
                var tableRange = sheet.Range($"A9:F{lastRow}"); // Thay đổi từ A8 thành A9
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                var starsRange = sheet.Range($"F10:F{lastRow}"); // Thay đổi từ F9 thành F10
                starsRange.AddConditionalFormat().WhenGreaterThan(4).Fill.SetBackgroundColor(XLColor.LightGreen);
                starsRange.AddConditionalFormat().WhenLessThan(3).Fill.SetBackgroundColor(XLColor.LightCoral);

                var titleRange = sheet.Range($"A10:A{lastRow}"); // Thay đổi từ A9 thành A10
                titleRange.Style.Fill.BackgroundColor = XLColor.LightCyan;
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }
        #endregion

        #region Authentication Methods

        private async Task<UserCredential> AuthenticateSheetAsync()
        {
            var credentialPath = Path.Combine(_googleAuthPath, "credentials.json");

            using var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read);
            var credPath = Path.Combine(_googleAuthPath, "token.sheets.json");

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { SheetsService.Scope.SpreadsheetsReadonly },
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));

            return credential;
        }

        #endregion

        #region Parsing and Extraction Methods

        // Phương thức để parse và validate toàn bộ summary
        private (string OrderCode, string Sale, string Title) ParseSummaryComplete(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
                return ("UNKNOWN", "Không rõ", "Không rõ");

            // Chuẩn hóa chuỗi
            summary = summary.Trim();
            summary = summary.Replace('–', '-').Replace('—', '-');
            summary = summary.Replace('_', '-').Replace('_', '-');
            summary = Regex.Replace(summary, @"\s*-\s*", "-");
            summary = Regex.Replace(summary, @"\s*_\s*", "-");

            var orderCode = ExtractOrderCodeSafe(summary);
            var sale = ExtractSaleFromSummary(summary);
            var title = ExtractTitleFromSummary(summary);

            return (orderCode, sale, title);
        }

        // Phương thức mới để trích xuất OrderCode một cách an toàn hơn
        private string ExtractOrderCodeSafe(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
                return "UNKNOWN";

            summary = summary.Trim();

            // Tìm OrderCode ở đầu chuỗi
            var match = Regex.Match(summary, @"^(DH\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.ToUpper();
            }

            // Tìm OrderCode ở bất kỳ đâu trong chuỗi
            match = Regex.Match(summary, @"\b(DH\d+)\b", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.ToUpper();
            }

            return "UNKNOWN";
        }

        // Sử dụng phương thức cải thiện cho ExtractOrderCode
        private string ExtractOrderCode(string summary)
        {
            return ExtractOrderCodeSafe(summary);
        }

        // Cải thiện phương thức trích xuất Title từ Summary
        private string ExtractTitleFromSummary(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
                return "Không rõ";

            // Loại bỏ khoảng trắng thừa và chuẩn hóa
            summary = summary.Trim();

            // Thay thế các ký tự tương tự nhau (en-dash, em-dash) thành dấu gạch ngang thông thường
            summary = summary.Replace('–', '-').Replace('—', '-');

            // Loại bỏ khoảng trắng thừa xung quanh dấu gạch ngang
            summary = Regex.Replace(summary, @"\s*-\s*", "-");

            try
            {
                // Pattern mới linh hoạt hơn để xử lý các trường hợp:
                // DH123-Sale-Title
                // DH123--Title (thiếu Sale)  
                // DH123-Sale (thiếu Title)
                // DH123-Sale-Title-ExtraInfo (có thông tin thêm)
                var patterns = new[]
                {
                    @"^DH\d+\s*-\s*([^-]+)\s*-\s*(.+)$",           // Định dạng đầy đủ: DH123-Sale-Title
                    @"^DH\d+\s*-\s*-\s*(.+)$",                     // Thiếu Sale: DH123--Title
                    @"^DH\d+\s*-\s*(.+?)\s*-\s*(.+)$",            // Fallback cho định dạng có 2 phần
                    @"^DH\d+\s*-\s*(.+)$"                          // Chỉ có 1 phần sau OrderCode: DH123-Something
                };

                foreach (var pattern in patterns)
                {
                    var match = Regex.Match(summary, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        if (match.Groups.Count == 3) // Có đầy đủ Sale và Title
                        {
                            var title = match.Groups[2].Value.Trim();
                            return !string.IsNullOrWhiteSpace(title) ? title : "Không rõ";
                        }
                        else if (match.Groups.Count == 2) // Chỉ có 1 phần
                        {
                            var part = match.Groups[1].Value.Trim();

                            // Nếu phần này có chứa dấu gạch ngang, có thể là Sale-Title
                            if (part.Contains("-"))
                            {
                                var subParts = part.Split('-', 2);
                                if (subParts.Length == 2)
                                {
                                    var potentialTitle = subParts[1].Trim();
                                    return !string.IsNullOrWhiteSpace(potentialTitle) ? potentialTitle : "Không rõ";
                                }
                            }

                            // Nếu không có dấu gạch ngang hoặc chỉ có 1 phần, coi như là Title
                            return !string.IsNullOrWhiteSpace(part) ? part : "Không rõ";
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Nếu regex fail, fallback về logic đơn giản
            }

            // Fallback: tách bằng dấu gạch ngang và lấy phần cuối
            var parts = summary.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                // Lấy tất cả phần từ phần thứ 3 trở đi và nối lại
                var titleParts = parts.Skip(2).Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p));
                var title = string.Join(" - ", titleParts);
                return !string.IsNullOrWhiteSpace(title) ? title : summary.Trim();
            }

            return summary.Trim();
        }

        // Cải thiện phương thức trích xuất Sale từ Summary
        private string ExtractSaleFromSummary(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
                return "Không rõ";

            // Loại bỏ khoảng trắng thừa và chuẩn hóa
            summary = summary.Trim();

            // Thay thế các ký tự tương tự nhau thành dấu gạch ngang thông thường
            summary = summary.Replace('–', '-').Replace('—', '-');

            // Loại bỏ khoảng trắng thừa xung quanh dấu gạch ngang
            summary = Regex.Replace(summary, @"\s*-\s*", "-");

            try
            {
                // Pattern để xử lý các trường hợp:
                var patterns = new[]
                {
                    @"^DH\d+\s*-\s*([^-]+)\s*-\s*.+$",             // Định dạng đầy đủ: DH123-Sale-Title
                    @"^DH\d+\s*-\s*([^-]+)$",                      // Chỉ có Sale: DH123-Sale
                    @"^DH\d+\s*-\s*-\s*.+$"                        // Thiếu Sale: DH123--Title
                };

                // Kiểm tra pattern thiếu Sale trước
                var missingSaleMatch = Regex.Match(summary, patterns[2], RegexOptions.IgnoreCase);
                if (missingSaleMatch.Success)
                {
                    return "Không rõ";
                }

                // Kiểm tra các pattern khác
                foreach (var pattern in patterns.Take(2))
                {
                    var match = Regex.Match(summary, pattern, RegexOptions.IgnoreCase);
                    if (match.Success && match.Groups.Count == 2)
                    {
                        var sale = match.Groups[1].Value.Trim();

                        // Kiểm tra xem phần này có phải là OrderCode bị lặp không
                        if (Regex.IsMatch(sale, @"^DH\d+$", RegexOptions.IgnoreCase))
                        {
                            continue;
                        }

                        return !string.IsNullOrWhiteSpace(sale) ? sale : "Không rõ";
                    }
                }
            }
            catch (Exception)
            {
                // Nếu regex fail, fallback về logic đơn giản
            }

            // Fallback: tách bằng dấu gạch ngang
            var parts = summary.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var salePart = parts[1].Trim();

                // Kiểm tra xem có phải OrderCode bị lặp không
                if (!Regex.IsMatch(salePart, @"^DH\d+$", RegexOptions.IgnoreCase) &&
                    !string.IsNullOrWhiteSpace(salePart))
                {
                    return salePart;
                }
            }

            return "Không rõ";
        }

        // Phương thức cải thiện ExtractQCFromDescription - Xử lý đầy đủ tất cả trường hợp QC
        private string ExtractQCFromDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return "Không rõ";

            try
            {
                // Chuẩn hóa HTML content
                var plainText = CleanHtmlContent(description);

                // Các pattern để tìm QC với độ linh hoạt cao - Xử lý đầy đủ tất cả trường hợp
                var qcPatterns = new[]
                {
                    // Pattern chính với ngày: - DD/MM/YYYY: QC-TenNguoi[v]
                    @"[-\s]*\d{1,2}\/\d{1,2}\/\d{2,4}\s*:\s*QC\s*[-:]?\s*([^[\]\n\r;]+?)\s*\[([vV])\]",
                    
                    // Pattern QC với ngày nhưng không có dấu gạch: DD/MM/YYYY: QC-TenNguoi[v]  
                    @"\d{1,2}\/\d{1,2}\/\d{2,4}\s*:\s*QC\s*[-:]?\s*([^[\]\n\r;]+?)\s*\[([vV])\]",
                    
                    // Pattern QC không có ngày: QC-TenNguoi[v] hoặc QC: TenNguoi[v]
                    @"QC\s*[-:]?\s*([^[\]\n\r;]+?)\s*\[([vV])\]",
                    
                    // Pattern QC có dấu chấm phẩy: QC-TenNguoi[v];
                    @"QC\s*[-:]?\s*([^[\]\n\r;]+?)\s*\[([vV])\]\s*;?",
                    
                    // Pattern QC chỉ có tên không có status: QC-TenNguoi; hoặc QC: TenNguoi
                    @"QC\s*[-:]?\s*([^[\]\n\r;:]+?)(?:\s*[;:]?\s*$|\s*[;:])",
                    
                    // Pattern QC trong timeline: bất kỳ dòng nào có QC
                    @"[-\s]*\d{1,2}\/\d{1,2}\/\d{2,4}.*?QC\s*[-:]?\s*([^[\]\n\r;]+?)(?:\s*\[([vV])\])?",
                    
                    // Pattern QC không có định dạng chuẩn: tìm "QC" và lấy text sau đó
                    @"QC\s*[-:]?\s*([A-Za-zÀ-ỹ\s]+?)(?=\s*[\[\n\r;:]|$)"
                };

                foreach (var pattern in qcPatterns)
                {
                    var matches = Regex.Matches(plainText, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (matches.Count > 0)
                    {
                        // Lấy match cuối cùng (QC gần nhất)
                        var lastMatch = matches[matches.Count - 1];

                        // Tìm group chứa tên QC
                        string qcName = "";
                        if (lastMatch.Groups.Count >= 2 && !string.IsNullOrWhiteSpace(lastMatch.Groups[1].Value))
                        {
                            qcName = lastMatch.Groups[1].Value;
                        }

                        if (!string.IsNullOrWhiteSpace(qcName))
                        {
                            // Chuẩn hóa tên QC
                            qcName = NormalizeQCName(qcName);

                            // Validate tên QC hợp lệ
                            if (IsValidQCName(qcName))
                            {
                                return qcName;
                            }
                        }
                    }
                }

                // Fallback: Tìm bất kỳ text nào sau chữ "QC" 
                var fallbackPatterns = new[]
                {
                    @"QC\s*[-:]?\s*([A-Za-zÀ-ỹ\s]{2,30})",
                    @".*QC.*?([A-Za-zÀ-ỹ\s]{3,25})"
                };

                foreach (var fallbackPattern in fallbackPatterns)
                {
                    var fallbackMatch = Regex.Match(plainText, fallbackPattern, RegexOptions.IgnoreCase);
                    if (fallbackMatch.Success && fallbackMatch.Groups.Count > 1)
                    {
                        var qcCandidate = fallbackMatch.Groups[1].Value.Trim();
                        qcCandidate = NormalizeQCName(qcCandidate);
                        if (IsValidQCName(qcCandidate))
                        {
                            return qcCandidate;
                        }
                    }
                }

                return "Không rõ";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting QC: {ex.Message}");
                return "Không rõ";
            }
        }

        #endregion

        #region Timeline Processing Methods

        private List<TimelineStep> ParseTimelineFromDescription(string descriptionHtml)
        {
            if (string.IsNullOrWhiteSpace(descriptionHtml))
                return new List<TimelineStep>();

            try
            {
                // Bước 1: Chuẩn hóa HTML và loại bỏ các tag HTML
                var plainText = CleanHtmlContent(descriptionHtml);

                var timelineSteps = new List<TimelineStep>();

                // Bước 2: Tìm phần TIMELINE (case-insensitive) hoặc bắt đầu parsing trực tiếp
                var timelineSection = ExtractTimelineSection(plainText);

                // DEBUG: Log để kiểm tra
                Console.WriteLine($"Timeline section: {timelineSection.Substring(0, Math.Min(100, timelineSection.Length))}...");

                // Bước 3: Tách timeline entries
                var timelineEntries = SplitTimelineEntries(timelineSection);

                // DEBUG: Log số lượng entries
                Console.WriteLine($"Found {timelineEntries.Count} timeline entries");

                // Bước 4: Parse từng entry
                foreach (var entry in timelineEntries)
                {
                    var cleanEntry = CleanTimelineLine(entry);
                    if (string.IsNullOrWhiteSpace(cleanEntry) || !ContainsDate(cleanEntry))
                        continue;

                    // DEBUG: Log entry đang parse
                    Console.WriteLine($"Parsing entry: {cleanEntry.Substring(0, Math.Min(50, cleanEntry.Length))}...");

                    var step = ParseTimelineEntryDirect(cleanEntry);
                    if (step != null && !string.IsNullOrWhiteSpace(step.Description))
                    {
                        timelineSteps.Add(step);
                        Console.WriteLine($"✓ Parsed: {step.Date:dd/MM/yyyy} - {step.Description.Substring(0, Math.Min(30, step.Description.Length))}... [{step.Status}]");
                    }
                    else
                    {
                        Console.WriteLine($"✗ Failed to parse entry");
                    }
                }

                // Bước 5: Xử lý logic status cascade (nếu bước sau hoàn thành thì bước trước cũng hoàn thành)
                ProcessStatusCascade(timelineSteps);

                // Bước 6: Sắp xếp theo ngày (ĐẢM BẢO ĐÚNG THỨ TỰ)
                return timelineSteps.OrderBy(t => t.Date).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing timeline: {ex.Message}");
                return new List<TimelineStep>();
            }
        }

        private List<string> SplitTimelineEntries(string timelineText)
        {
            if (string.IsNullOrWhiteSpace(timelineText))
                return new List<string>();

            var entries = new List<string>();

            // Trước tiên thử tách bằng line breaks thông thường
            var lines = timelineText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Nếu chỉ có 1 dòng (không có line breaks), tách bằng pattern "- DD/MM/YYYY:"
            if (lines.Length == 1)
            {
                // FIXED: Tìm tất cả vị trí bắt đầu của "- DD/MM/YYYY:" và tách chính xác
                var datePattern = @"- (\d{1,2}\/\d{1,2}\/\d{2,4}):";
                var matches = Regex.Matches(timelineText, datePattern);

                if (matches.Count > 0)
                {
                    // Tách timeline từ vị trí match đến match tiếp theo
                    for (int i = 0; i < matches.Count; i++)
                    {
                        var startIndex = matches[i].Index;
                        var endIndex = (i + 1 < matches.Count) ? matches[i + 1].Index : timelineText.Length;

                        var entry = timelineText.Substring(startIndex, endIndex - startIndex).Trim();
                        if (!string.IsNullOrWhiteSpace(entry))
                        {
                            entries.Add(entry);
                        }
                    }
                }
                else
                {
                    // Nếu không tìm thấy pattern, return toàn bộ text
                    entries.Add(timelineText.Trim());
                }
            }
            else
            {
                // Nếu có line breaks, sử dụng cách tách thông thường
                entries = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
            }

            return entries;
        }

        private TimelineStep? ParseTimelineEntryDirect(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry))
                return null;

            try
            {
                // Pattern đơn giản và chính xác: - DD/MM/YYYY: Description[status]
                var pattern = @"^-?\s*(\d{1,2}\/\d{1,2}\/\d{2,4})\s*:\s*(.*?)(\[([vVxX])\])?\s*$";
                var match = Regex.Match(entry.Trim(), pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    var dateStr = match.Groups[1].Value.Trim();
                    var description = match.Groups[2].Value.Trim();
                    var status = match.Groups.Count > 4 && !string.IsNullOrEmpty(match.Groups[4].Value)
                                ? match.Groups[4].Value.ToUpper()
                                : null;

                    // Parse date
                    var parsedDate = ParseDateFlexible(dateStr);
                    if (parsedDate.HasValue)
                    {
                        return new TimelineStep
                        {
                            Date = parsedDate.Value,
                            Description = description,
                            Status = status
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing entry '{entry}': {ex.Message}");
            }

            return null;
        }

        #endregion

        #region Helper Methods
        private double CalculateRewardFromConfig(decimal averageStars, decimal hssl, decimal penalty, KpiConfiguration config)
        {
            decimal reward = 0;

            if (averageStars >= config.Reward_HighPerformance_MinStars && averageStars <= config.Reward_HighPerformance_MaxStars)
            {
                // Công thức thưởng: (TD-3)*2500000/2-500000 – PB * HSSL
                reward = ((averageStars - config.Penalty_MediumPerformance_MinStars) * config.Reward_BaseAmount / 2 - config.Reward_BasePenalty - penalty) * hssl;
            }
            else if (averageStars >= config.Penalty_MediumPerformance_MinStars && averageStars < config.Penalty_MediumPerformance_MaxStars)
            {
                // Công thức phạt nhẹ: (TD-3)*2500000/2-500000 + PB
                reward = (averageStars - config.Penalty_MediumPerformance_MinStars) * config.Reward_BaseAmount / 2 - config.Reward_BasePenalty + penalty;
            }
            else if (averageStars >= config.Penalty_LowPerformance_MinStars && averageStars < config.Penalty_LowPerformance_MaxStars)
            {
                // Công thức phạt nặng: (TD-1)*500000/2-1000000 + PB
                reward = (averageStars - config.Penalty_LowPerformance_MinStars) * config.Penalty_LowPerformance_BaseAmount / 2 - config.Penalty_LowPerformance_MaxPenalty + penalty;
            }

            return (double)reward;
        }
        private double CalculateHSSLFromConfig(int lightOrders, int mediumOrders, int heavyOrders, KpiConfiguration config)
        {
            // HSSL = [MAX(SLn – FreeCount, 0)] * LightMultiplier + SLv * MediumMultiplier + SLl * HeavyMultiplier
            var excessLightOrders = Math.Max(lightOrders - config.HSSL_LightOrderFreeCount, 0);
            var hssl = excessLightOrders * (double)config.HSSL_LightOrderMultiplier +
                      mediumOrders * (double)config.HSSL_MediumOrderMultiplier +
                      heavyOrders * (double)config.HSSL_HeavyOrderMultiplier;

            return hssl;
        }

        private (int light, int medium, int heavy) CategorizeOrdersFromConfig(List<int> orderDays, KpiConfiguration config)
        {
            var light = orderDays.Count(days => days < config.LightOrder_MaxDays);
            var medium = orderDays.Count(days => days >= config.MediumOrder_MinDays && days <= config.MediumOrder_MaxDays);
            var heavy = orderDays.Count(days => days >= config.HeavyOrder_MinDays);

            return (light, medium, heavy);
        }
        private int CalculateStarsFromConfig(int daysLate, KpiConfiguration config)
        {
            return daysLate switch
            {
                < 0 => config.Stars_EarlyCompletion,  // Hoàn thành sớm
                0 => config.Stars_OnTime,             // Đúng hạn
                1 => config.Stars_Late1Day,           // Trễ 1 ngày
                2 => config.Stars_Late2Days,          // Trễ 2 ngày
                _ => config.Stars_Late3OrMoreDays     // Trễ 3+ ngày
            };
        }
        // Phương thức helper để chuẩn hóa HTML content
        private string CleanHtmlContent(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
                return "";

            // Loại bỏ HTML tags
            var plainText = Regex.Replace(htmlContent, @"<[^>]*>", "");

            // Decode HTML entities
            plainText = System.Net.WebUtility.HtmlDecode(plainText);

            // Chuẩn hóa line breaks
            plainText = plainText.Replace("<br>", "\n")
                                .Replace("<br/>", "\n")
                                .Replace("<br />", "\n");

            // Loại bỏ khoảng trắng thừa nhưng giữ line breaks
            plainText = Regex.Replace(plainText, @"[ \t]+", " ");

            return plainText.Trim();
        }

        // Phương thức helper để extract timeline section
        private string ExtractTimelineSection(string content)
        {
            // Tìm phần TIMELINE nếu có (case-insensitive)
            var timelineMatch = Regex.Match(content, @"timeline\s*\n?(.*?)(?=\n\n|\n[A-Z]+|$)",
                                          RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (timelineMatch.Success && !string.IsNullOrWhiteSpace(timelineMatch.Groups[1].Value))
            {
                return timelineMatch.Groups[1].Value.Trim();
            }

            // Nếu không có phần TIMELINE rõ ràng, return toàn bộ content
            return content.Trim();
        }

        // Phương thức helper để clean timeline line - loại bỏ dấu gạch thừa và ký tự đặc biệt
        private string CleanTimelineLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return "";

            // Loại bỏ các ký tự đặc biệt ở đầu và cuối dòng
            line = line.Trim();

            // Loại bỏ dòng chỉ chứa dấu gạch ngang
            if (Regex.IsMatch(line, @"^[-\s]*$"))
                return "";

            // Loại bỏ dấu gạch ngang thừa ở đầu nhưng giữ lại dấu gạch đầu tiên
            line = Regex.Replace(line, @"^[-]{2,}", "-");

            // Loại bỏ khoảng trắng thừa
            line = Regex.Replace(line, @"\s+", " ");

            return line.Trim();
        }

        // Phương thức helper để kiểm tra có chứa ngày không
        private bool ContainsDate(string line)
        {
            return Regex.IsMatch(line, @"\d{1,2}\/\d{1,2}\/\d{2,4}");
        }

        // Phương thức helper để parse một timeline step từ regex match
        private TimelineStep? ParseTimelineStep(Match match, string originalLine)
        {
            try
            {
                if (match.Groups.Count < 3)
                    return null;

                var dateStr = match.Groups[1].Value.Trim();
                var description = match.Groups[2].Value.Trim();
                string? status = null;

                // Tìm status từ các group
                if (match.Groups.Count > 4 && !string.IsNullOrWhiteSpace(match.Groups[4].Value))
                {
                    status = match.Groups[4].Value.ToUpper();
                }
                else if (match.Groups.Count > 3 && !string.IsNullOrWhiteSpace(match.Groups[3].Value))
                {
                    // Tìm status từ phần cuối description
                    var statusMatch = Regex.Match(description, @"\[([vVxX])\]$", RegexOptions.IgnoreCase);
                    if (statusMatch.Success)
                    {
                        status = statusMatch.Groups[1].Value.ToUpper();
                        description = description.Substring(0, statusMatch.Index).Trim();
                    }
                }

                // Parse date với nhiều định dạng
                var parsedDate = ParseDateFlexible(dateStr);
                if (parsedDate == null)
                    return null;

                return new TimelineStep
                {
                    Date = parsedDate.Value,
                    Description = description,
                    Status = status
                };
            }
            catch
            {
                return null;
            }
        }
        private decimal ExtractPenaltyFromDescription(string description, KpiConfiguration kpiConfig)
        {
            if (string.IsNullOrWhiteSpace(description))
                return kpiConfig.Penalty_NoError; // Mặc định không lỗi

            try
            {
                // Chuẩn hóa HTML content
                var plainText = CleanHtmlContent(description);

                // Tìm pattern PB:số
                var pbPattern = @"PB\s*:\s*(\d+)";
                var match = Regex.Match(plainText, pbPattern, RegexOptions.IgnoreCase);

                if (match.Success && int.TryParse(match.Groups[1].Value, out int pbValue))
                {
                    return pbValue switch
                    {
                        100 => kpiConfig.Penalty_LightError,  // Lỗi nhẹ
                        500 => kpiConfig.Penalty_HeavyError,  // Lỗi nặng
                        0 => kpiConfig.Penalty_NoError,       // Không lỗi
                        _ => pbValue >= 500 ? kpiConfig.Penalty_HeavyError : kpiConfig.Penalty_LightError
                    };
                }

                // Nếu không tìm thấy PB, mặc định không lỗi
                return kpiConfig.Penalty_NoError;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting penalty from description: {ex.Message}");
                return kpiConfig.Penalty_NoError;
            }
        }
        // Phương thức fallback để parse timeline step
        private TimelineStep? ParseTimelineStepFallback(string line)
        {
            try
            {
                // Thử tìm ngày trong line bằng pattern đơn giản
                var dateMatch = Regex.Match(line, @"(\d{1,2}\/\d{1,2}\/\d{2,4})");
                if (!dateMatch.Success)
                    return null;

                var dateStr = dateMatch.Groups[1].Value;
                var parsedDate = ParseDateFlexible(dateStr);
                if (parsedDate == null)
                    return null;

                // Lấy phần description sau ngày
                var afterDate = line.Substring(dateMatch.Index + dateMatch.Length);
                afterDate = afterDate.TrimStart(':', ' ', '-').Trim();

                // Tìm status
                string? status = null;
                var statusMatch = Regex.Match(afterDate, @"\[([vVxX])\]", RegexOptions.IgnoreCase);
                if (statusMatch.Success)
                {
                    status = statusMatch.Groups[1].Value.ToUpper();
                    afterDate = afterDate.Substring(0, statusMatch.Index).Trim();
                }

                if (string.IsNullOrWhiteSpace(afterDate))
                    return null;

                return new TimelineStep
                {
                    Date = parsedDate.Value,
                    Description = afterDate,
                    Status = status
                };
            }
            catch
            {
                return null;
            }
        }

        // Phương thức parse date linh hoạt
        private DateTime? ParseDateFlexible(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return null;

            var formats = new[]
            {
                "dd/MM/yyyy", "d/MM/yyyy", "dd/M/yyyy", "d/M/yyyy",
                "dd-MM-yyyy", "d-MM-yyyy", "dd-M-yyyy", "d-M-yyyy",
                "yyyy-MM-dd", "yyyy/MM/dd",
                "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy", "M/d/yyyy",
                "dd/MM/yy", "d/MM/yy", "dd/M/yy", "d/M/yy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateStr.Trim(), format, null,
                                         System.Globalization.DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }

            // Fallback với DateTime.TryParse
            if (DateTime.TryParse(dateStr.Trim(), out var fallback))
            {
                return fallback;
            }

            return null;
        }

        // Phương thức xử lý logic cascade status
        private void ProcessStatusCascade(List<TimelineStep> timelineSteps)
        {
            if (timelineSteps.Count <= 1)
                return;

            // Sắp xếp theo ngày trước khi xử lý
            timelineSteps.Sort((a, b) => a.Date.CompareTo(b.Date));

            // Logic: nếu bước sau có status "V" và bước trước có status "X" hoặc null
            // thì cập nhật bước trước thành "V"
            for (int i = timelineSteps.Count - 1; i > 0; i--)
            {
                if (timelineSteps[i].Status == "V" &&
                    (timelineSteps[i - 1].Status == "X" || timelineSteps[i - 1].Status == null))
                {
                    timelineSteps[i - 1].Status = "V";
                }
            }
        }

        // Phương thức chuẩn hóa tên QC
        private string NormalizeQCName(string qcName)
        {
            if (string.IsNullOrWhiteSpace(qcName))
                return "";

            // Chuẩn hóa tên QC
            qcName = qcName.Trim()
                           .Replace("\n", " ")
                           .Replace("\r", " ")
                           .Replace("\t", " ");

            // Loại bỏ khoảng trắng thừa
            qcName = Regex.Replace(qcName, @"\s+", " ").Trim();

            // Loại bỏ ký tự đặc biệt ở đầu và cuối
            qcName = qcName.Trim('-', ':', ';', ' ');

            return qcName;
        }

        // Phương thức validate tên QC hợp lệ
        private bool IsValidQCName(string qcName)
        {
            if (string.IsNullOrWhiteSpace(qcName))
                return false;

            // Kiểm tra độ dài hợp lý
            if (qcName.Length < 2 || qcName.Length > 50)
                return false;

            // Kiểm tra có chứa ít nhất một chữ cái
            if (!Regex.IsMatch(qcName, @"[A-Za-zÀ-ỹ]"))
                return false;

            // Loại bỏ các từ khóa không phải tên người
            var invalidKeywords = new[] { "QC", "TEST", "DEBUG", "TEMP", "NULL", "NONE", "UNKNOWN" };
            if (invalidKeywords.Any(keyword => qcName.ToUpper().Contains(keyword)))
                return false;

            return true;
        }

        private static string ConvertColorToHex(Color color)
        {
            if (color == null) return "#FFFFFF";
            int r = (int)((color.Red ?? 0) * 255);
            int g = (int)((color.Green ?? 0) * 255);
            int b = (int)((color.Blue ?? 0) * 255);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        private static string MapColorToStatus(string hexColor)
        {
            return hexColor.ToUpper() switch
            {
                "#6AA84F" => "BÌNH THƯỜNG",
                "#FFFF00" => "CẬN NGÀY",
                "#E69138" => "TỚI NGÀY",
                "#FF0000" => "TRỄ NGÀY",
                "#3C78D8" => "HOÀN THÀNH",
                _ => "KHÔNG XÁC ĐỊNH"
            };
        }

        private static DateTime? ParseDate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var formats = new[]
            {
                "dd/MM/yyyy", "dd-MM-yyyy", "dd-MM-yy",
                "yyyy-MM-dd", "MM/dd/yyyy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(input, format, null, System.Globalization.DateTimeStyles.None, out var result))
                    return result;
            }

            if (DateTime.TryParse(input, out var fallback))
                return fallback;

            return null;
        }

        public async Task<OrderEventDto> MapEventToOrderDto(Event ev)
        {
            var userCalendarList = (await _repository.UserCalendar.GetUserCalendarsAsync(false)).ToList();
            var userCalendarDto = _mapper.Map<IEnumerable<UserCalendarDto>>(userCalendarList);
            var userCal = userCalendarDto.FirstOrDefault(x => string.Equals(x.Name, ev.Organizer?.DisplayName ?? "Không rõ", StringComparison.OrdinalIgnoreCase));

            // Sử dụng phương thức cải thiện để parse
            var (orderCode, sale, title) = ParseSummaryComplete(ev.Summary);
            var qc = ExtractQCFromDescription(ev.Description ?? "");
            var orderDto = new OrderEventDto();
            if (ev.Start.Date!=null&& ev.Start.Date!=null)
            {
                orderDto = new OrderEventDto
                {
                    Title = title,
                    OrderCode = orderCode,
                    StartDate = DateTime.Parse(ev.Start.Date),
                    EndDate = DateTime.Parse(ev.End.Date).AddDays(-1),
                    Handler = userCal?.UserName ?? "Không rõ",
                    Timeline = ParseTimelineFromDescription(ev.Description ?? ""),
                    Sale = sale,
                    QC = qc
                };
            }
            else
            {
                orderDto = new OrderEventDto
                {
                    Title = title,
                    OrderCode = orderCode,
                    StartDate = ev.Start.DateTimeDateTimeOffset?.DateTime ?? DateTime.MinValue,
                    EndDate = ev.End.DateTimeDateTimeOffset?.DateTime ?? DateTime.MinValue,
                    Handler = userCal?.UserName ?? "Không rõ",
                    Timeline = ParseTimelineFromDescription(ev.Description ?? ""),
                    Sale = sale,
                    QC = qc
                };
            }    

            

            return orderDto;
        }

        #endregion
    }
}
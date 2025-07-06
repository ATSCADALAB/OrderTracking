using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Hangfire;
using QuickStart.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Claims;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/calendar-report")]
    [ApiController]

    public class CalendarReportController : ControllerBase
    {
        private readonly IServiceManager _service;
        private readonly ILogger<CalendarReportController> _logger;

        public CalendarReportController(IServiceManager service, ILogger<CalendarReportController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Chỉ cần lấy userId từ JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }
            var fileBytes = await _service.CalendarReportService.GenerateReportAsync(startDate, endDate, userId);
            var fileName = $"user_order_report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("kpi-summary")]
        [Authorize]
        public async Task<IActionResult> GetKpiSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {

            // Chỉ cần lấy userId từ JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            // Service tự lấy role của user này từ database
            var result = await _service.CalendarReportService.GetUserKpiReportAsync(startDate, endDate, userId);

            return Ok(result);
        }

        [HttpGet("search-by-order")]
        public async Task<IActionResult> SearchByOrderCode([FromQuery] string orderCode)
        {
            var orderResult = await _service.CalendarReportService.SearchOrderFromAllSourcesAsync(orderCode);
            if (orderResult == null)
                return Ok(null);
            return Ok(orderResult);
        }

        [HttpGet("syncsheet")]
        public async Task<IActionResult> SyncSheet()
        {
            await _service.CalendarReportService.SyncSheetOrdersAsync(CancellationToken.None);
            return Ok(new { Success = true, Message = "Đồng bộ Sheet hoàn thành", Timestamp = DateTime.Now });
        }

        [HttpGet("synccalendar")]
        public async Task<IActionResult> SyncCalendar()
        {
            await _service.CalendarReportService.SyncCalendarEventsAsync(CancellationToken.None);
            return Ok(new { Success = true, Message = "Đồng bộ Calendar hoàn thành", Timestamp = DateTime.Now });
        }

        // ===== HANGFIRE JOB MANAGEMENT ENDPOINTS =====

        /// <summary>
        /// Chạy đồng bộ hàng tháng ngay lập tức (không cần chờ đến lịch trình)
        /// </summary>
        [HttpPost("jobs/trigger-monthly-sync")]
        public IActionResult TriggerMonthlySyncNow()
        {
            try
            {
                var jobId = BackgroundJob.Enqueue<HangfireSyncService>(service =>
                    service.ExecuteMonthlySyncAsync());

                _logger.LogInformation("Đã khởi chạy Monthly Sync Job: {JobId}", jobId);

                return Ok(new
                {
                    Success = true,
                    Message = "Đã khởi chạy job đồng bộ hàng tháng",
                    JobId = jobId,
                    Timestamp = DateTime.Now,
                    Note = "Kiểm tra tiến trình tại /hangfire"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khởi chạy Monthly Sync Job");
                return StatusCode(500, new { Success = false, Message = "Lỗi khi khởi chạy job", Error = ex.Message });
            }
        }

        /// <summary>
        /// Chạy đồng bộ Sheet ngay lập tức
        /// </summary>
        [HttpPost("jobs/trigger-sync-sheet")]
        public IActionResult TriggerSyncSheetNow()
        {
            try
            {
                var jobId = BackgroundJob.Enqueue<HangfireSyncService>(service =>
                    service.ExecuteSyncSheetAsync());

                _logger.LogInformation("Đã khởi chạy Sync Sheet Job: {JobId}", jobId);

                return Ok(new
                {
                    Success = true,
                    Message = "Đã khởi chạy job đồng bộ Sheet",
                    JobId = jobId,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khởi chạy Sync Sheet Job");
                return StatusCode(500, new { Success = false, Message = "Lỗi khi khởi chạy job", Error = ex.Message });
            }
        }

        /// <summary>
        /// Chạy đồng bộ Calendar ngay lập tức
        /// </summary>
        [HttpPost("jobs/trigger-sync-calendar")]
        public IActionResult TriggerSyncCalendarNow()
        {
            try
            {
                var jobId = BackgroundJob.Enqueue<HangfireSyncService>(service =>
                    service.ExecuteSyncCalendarAsync());

                _logger.LogInformation("Đã khởi chạy Sync Calendar Job: {JobId}", jobId);

                return Ok(new
                {
                    Success = true,
                    Message = "Đã khởi chạy job đồng bộ Calendar",
                    JobId = jobId,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khởi chạy Sync Calendar Job");
                return StatusCode(500, new { Success = false, Message = "Lỗi khi khởi chạy job", Error = ex.Message });
            }
        }

        /// <summary>
        /// Xem thông tin về các jobs và lịch trình
        /// </summary>
        [HttpGet("jobs/status")]
        public IActionResult GetJobsStatus()
        {
            try
            {
                var nextMonthlyRun = GetNextMonthlyRun();

                return Ok(new
                {
                    Jobs = new
                    {
                        MonthlySync = new
                        {
                            Id = "monthly-full-sync",
                            Description = "Đồng bộ toàn bộ (Sheet + Calendar)",
                            Schedule = "Ngày 1 hàng tháng lúc 2:00 AM (GMT+7)",
                            NextRun = nextMonthlyRun,
                            Status = "Active"
                        },
                        SyncSheetOnly = new
                        {
                            Id = "sync-sheet-only",
                            Description = "Chỉ đồng bộ Sheet Orders",
                            Schedule = "Manual only",
                            Status = "Manual"
                        },
                        SyncCalendarOnly = new
                        {
                            Id = "sync-calendar-only",
                            Description = "Chỉ đồng bộ Calendar Events",
                            Schedule = "Manual only",
                            Status = "Manual"
                        }
                    },
                    Dashboard = new
                    {
                        Url = "/hangfire",
                        Description = "Truy cập dashboard để xem chi tiết jobs"
                    },
                    ServerTime = DateTime.Now,
                    TimeZone = "SE Asia Standard Time (GMT+7)"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin jobs");
                return StatusCode(500, new { Success = false, Message = "Lỗi khi lấy thông tin", Error = ex.Message });
            }
        }

        /// <summary>
        /// Đặt lại lịch trình cho job đồng bộ hàng tháng
        /// </summary>
        [HttpPost("jobs/reschedule-monthly")]
        public IActionResult RescheduleMonthlyJob([FromQuery] int day = 1, [FromQuery] int hour = 2)
        {
            try
            {
                if (day < 1 || day > 28)
                {
                    return BadRequest(new { Success = false, Message = "Ngày phải từ 1-28" });
                }

                if (hour < 0 || hour > 23)
                {
                    return BadRequest(new { Success = false, Message = "Giờ phải từ 0-23" });
                }

                RecurringJob.AddOrUpdate<HangfireSyncService>(
                    "monthly-full-sync",
                    service => service.ExecuteMonthlySyncAsync(),
                    Cron.Monthly(day, hour),
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                );

                var nextRun = GetNextMonthlyRun(day, hour);

                _logger.LogInformation("Đã đặt lại lịch Monthly Sync: ngày {Day} lúc {Hour}:00", day, hour);

                return Ok(new
                {
                    Success = true,
                    Message = $"Đã đặt lại lịch: ngày {day} hàng tháng lúc {hour:D2}:00",
                    NewSchedule = new
                    {
                        Day = day,
                        Hour = hour,
                        NextRun = nextRun,
                        CronExpression = $"0 0 {hour} {day} * ?"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt lại lịch Monthly Sync");
                return StatusCode(500, new { Success = false, Message = "Lỗi khi đặt lại lịch", Error = ex.Message });
            }
        }

        /// <summary>
        /// Tạm dừng job đồng bộ hàng tháng
        /// </summary>
        [HttpPost("jobs/pause-monthly")]
        public IActionResult PauseMonthlyJob()
        {
            try
            {
                RecurringJob.RemoveIfExists("monthly-full-sync");

                _logger.LogInformation("Đã tạm dừng Monthly Sync Job");

                return Ok(new
                {
                    Success = true,
                    Message = "Đã tạm dừng job đồng bộ hàng tháng",
                    Note = "Sử dụng /jobs/resume-monthly để kích hoạt lại"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạm dừng Monthly Sync Job");
                return StatusCode(500, new { Success = false, Message = "Lỗi khi tạm dừng job", Error = ex.Message });
            }
        }

        /// <summary>
        /// Kích hoạt lại job đồng bộ hàng tháng
        /// </summary>
        [HttpPost("jobs/resume-monthly")]
        public IActionResult ResumeMonthlyJob()
        {
            try
            {
                RecurringJob.AddOrUpdate<HangfireSyncService>(
                    "monthly-full-sync",
                    service => service.ExecuteMonthlySyncAsync(),
                    Cron.Monthly(1, 2),
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                );

                _logger.LogInformation("Đã kích hoạt lại Monthly Sync Job");

                return Ok(new
                {
                    Success = true,
                    Message = "Đã kích hoạt lại job đồng bộ hàng tháng",
                    Schedule = "Ngày 1 hàng tháng lúc 2:00 AM (GMT+7)",
                    NextRun = GetNextMonthlyRun()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kích hoạt lại Monthly Sync Job");
                return StatusCode(500, new { Success = false, Message = "Lỗi khi kích hoạt lại job", Error = ex.Message });
            }
        }

        /// <summary>
        /// Lập lịch chạy job một lần vào thời điểm cụ thể
        /// </summary>
        [HttpPost("jobs/schedule-one-time")]
        public IActionResult ScheduleOneTimeSync([FromQuery] DateTime scheduleTime, [FromQuery] string jobType = "full")
        {
            try
            {
                if (scheduleTime <= DateTime.Now)
                {
                    return BadRequest(new { Success = false, Message = "Thời gian phải ở tương lai" });
                }

                string jobId;
                string message;

                switch (jobType.ToLower())
                {
                    case "sheet":
                        jobId = BackgroundJob.Schedule<HangfireSyncService>(
                            service => service.ExecuteSyncSheetAsync(),
                            scheduleTime);
                        message = "Đã lập lịch đồng bộ Sheet";
                        break;
                    case "calendar":
                        jobId = BackgroundJob.Schedule<HangfireSyncService>(
                            service => service.ExecuteSyncCalendarAsync(),
                            scheduleTime);
                        message = "Đã lập lịch đồng bộ Calendar";
                        break;
                    default:
                        jobId = BackgroundJob.Schedule<HangfireSyncService>(
                            service => service.ExecuteMonthlySyncAsync(),
                            scheduleTime);
                        message = "Đã lập lịch đồng bộ toàn bộ";
                        break;
                }

                _logger.LogInformation("Đã lập lịch {JobType} Job: {JobId} vào {ScheduleTime}", jobType, jobId, scheduleTime);

                return Ok(new
                {
                    Success = true,
                    Message = message,
                    JobId = jobId,
                    ScheduledTime = scheduleTime,
                    JobType = jobType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lập lịch job");
                return StatusCode(500, new { Success = false, Message = "Lỗi khi lập lịch", Error = ex.Message });
            }
        }

        // ===== PRIVATE HELPER METHODS =====

        private DateTime GetNextMonthlyRun(int day = 1, int hour = 2)
        {
            var now = DateTime.Now;
            var thisMonth = new DateTime(now.Year, now.Month, day, hour, 0, 0);

            if (thisMonth <= now)
            {
                return thisMonth.AddMonths(1);
            }
            return thisMonth;
        }
    }
}
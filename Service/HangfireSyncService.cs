using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Contracts;

namespace QuickStart.Services
{

    public class HangfireSyncService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<HangfireSyncService> _logger;

        public HangfireSyncService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<HangfireSyncService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task ExecuteMonthlySyncAsync()
        {
            _logger.LogInformation("🚀 Bắt đầu đồng bộ hàng tháng với Hangfire - {Timestamp}", DateTime.Now);

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var serviceManager = scope.ServiceProvider.GetRequiredService<IServiceManager>();

                // Đồng bộ Sheet Orders
                _logger.LogInformation("📊 Đang đồng bộ Sheet Orders...");
                await serviceManager.CalendarReportService.SyncSheetOrdersAsync(CancellationToken.None);
                _logger.LogInformation("✅ Hoàn thành đồng bộ Sheet Orders");

                // Đồng bộ Calendar Events
                _logger.LogInformation("📅 Đang đồng bộ Calendar Events...");
                await serviceManager.CalendarReportService.SyncCalendarEventsAsync(CancellationToken.None);
                _logger.LogInformation("✅ Hoàn thành đồng bộ Calendar Events");

                _logger.LogInformation("🎉 Hoàn thành toàn bộ đồng bộ hàng tháng - {Timestamp}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi thực hiện đồng bộ hàng tháng");
                throw; // Để Hangfire biết job thất bại và có thể retry
            }
        }

        public async Task ExecuteSyncSheetAsync()
        {
            _logger.LogInformation("📊 Bắt đầu đồng bộ Sheet Orders...");

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var serviceManager = scope.ServiceProvider.GetRequiredService<IServiceManager>();

                await serviceManager.CalendarReportService.SyncSheetOrdersAsync(CancellationToken.None);
                _logger.LogInformation("✅ Hoàn thành đồng bộ Sheet Orders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi đồng bộ Sheet Orders");
                throw;
            }
        }

        public async Task ExecuteSyncCalendarAsync()
        {
            _logger.LogInformation("📅 Bắt đầu đồng bộ Calendar Events...");

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var serviceManager = scope.ServiceProvider.GetRequiredService<IServiceManager>();

                await serviceManager.CalendarReportService.SyncCalendarEventsAsync(CancellationToken.None);
                _logger.LogInformation("✅ Hoàn thành đồng bộ Calendar Events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi đồng bộ Calendar Events");
                throw;
            }
        }
    }
}
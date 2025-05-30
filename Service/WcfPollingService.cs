// QuickStart/Service/WcfPollingService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStart.Service
{
    public class WcfPollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WcfPollingService> _logger;

        public WcfPollingService(IServiceScopeFactory scopeFactory, ILogger<WcfPollingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WCF Polling Service is starting.");

            // Tạo scope để lấy IServiceManager
            using (var scope = _scopeFactory.CreateScope())
            {
                var serviceManager = scope.ServiceProvider.GetRequiredService<IServiceManager>();

                try
                {
                    // Bắt đầu polling từ WCF và đẩy qua SignalR
                    await serviceManager.WcfService.StartPollingAsync(new[] { "DemoNewDevice.Value" }, 2000);
                    _logger.LogInformation("WCF polling started successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while starting WCF polling.");
                }
            }

            // Giữ service chạy cho đến khi ứng dụng dừng
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("WCF Polling Service is stopping.");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WCF Polling Service is stopping.");
            using (var scope = _scopeFactory.CreateScope())
            {
                var serviceManager = scope.ServiceProvider.GetRequiredService<IServiceManager>();
                await serviceManager.WcfService.StopPollingAsync();
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
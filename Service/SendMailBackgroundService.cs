using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service;
using Service.Contracts;

namespace QuickStart.Services
{
    public class SendMailBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SendMailBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1); // 120 phút

        public SendMailBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<SendMailBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SendMail Background Service đã bắt đầu chạy mỗi 120 phút");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();

                    _logger.LogInformation("Bắt đầu xử lý đơn hàng mới - {Timestamp}", DateTime.Now);

                    await sendMailService.ProcessNewOrdersAsync();

                    _logger.LogInformation("Hoàn thành xử lý đơn hàng mới - {Timestamp}", DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xử lý SendMail Background Service");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SendMail Background Service đang dừng lại");
            await base.StopAsync(stoppingToken);
        }
    }
}
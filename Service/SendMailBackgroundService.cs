using EmailService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Contracts;

namespace QuickStart.Services
{
    public class SendMailBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SendMailBackgroundService> _logger;
        private readonly EmailSettings _emailSettings;

        public SendMailBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<SendMailBackgroundService> logger,
            IOptions<EmailSettings> emailSettings)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 SendMail Background Service started with {Hours}-hour interval", _emailSettings.ProcessingIntervalHours);

            // ✅ DELAY KHỞI ĐỘNG 10 PHÚT ĐỂ TRÁNH STARTUP RUSH
            await Task.Delay(TimeSpan.FromMinutes(100), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();

                    _logger.LogInformation("📧 Starting email processing batch - {Timestamp}", DateTime.Now);

                    await sendMailService.ProcessNewOrdersAsync();

                    _logger.LogInformation("✅ Completed email processing batch - {Timestamp}", DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in SendMail Background Service");
                }

                // ✅ RANDOM DELAY ĐỂ TRÁNH PREDICTABLE PATTERN
                var baseInterval = TimeSpan.FromHours(_emailSettings.ProcessingIntervalHours);
                var randomDelay = TimeSpan.FromMinutes(Random.Shared.Next(1, 5));
                var totalDelay = baseInterval + randomDelay;

                _logger.LogInformation("⏰ Next email processing in {TotalMinutes} minutes", totalDelay.TotalMinutes);
                await Task.Delay(totalDelay, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🛑 SendMail Background Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
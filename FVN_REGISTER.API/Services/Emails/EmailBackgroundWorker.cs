using FVN_REGISTER.Contract.Interfaces.Emails;

namespace FVN_REGISTER.API.Services.Emails
{
    public class EmailBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailBackgroundWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(5);

        public EmailBackgroundWorker(
            IServiceProvider serviceProvider,
            ILogger<EmailBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Background Worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Email queue processing at {Time}", DateTimeOffset.Now);

                    using var scope = _serviceProvider.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    await emailService.ProcessQueue(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Email worker cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing email queue");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Email Background Worker stopped");
        }
    }
}

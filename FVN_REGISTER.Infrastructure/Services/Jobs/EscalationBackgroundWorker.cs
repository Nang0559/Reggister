

using FVN_REGISTER.Application.Interfaces.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Jobs
{
    // Cần tạo file: EscalationBackgroundWorker.cs
    public class EscalationBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EscalationBackgroundWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(4);

        public EscalationBackgroundWorker(
            IServiceProvider serviceProvider,
            ILogger<EscalationBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[ESC_WORKER] Started, interval={Hours}h", _period.TotalHours);

            // Chạy ngay lần đầu
            await DoWorkAsync(stoppingToken);

            try
            {
                using var timer = new PeriodicTimer(_period);
                while (!stoppingToken.IsCancellationRequested
                       && await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWorkAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // App đang shutdown, bỏ qua
            }

            _logger.LogInformation("[ESC_WORKER] Stopped");
        }

        private async Task DoWorkAsync(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("[ESC_WORKER] Running at {Time}", DateTimeOffset.Now);

                await using var scope = _serviceProvider.CreateAsyncScope();
                var escalationService = scope.ServiceProvider.GetRequiredService<ILeaveEscalationService>();
                await escalationService.ProcessAsync(ct);

                _logger.LogInformation("[ESC_WORKER] Completed at {Time}", DateTimeOffset.Now);
            }
            catch (OperationCanceledException)
            {
                // Bỏ qua khi shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ESC_WORKER] Error during execution");
            }
        }
    }
}

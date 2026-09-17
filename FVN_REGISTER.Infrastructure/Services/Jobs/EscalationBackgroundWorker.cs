using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.OT;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Jobs
{
    /// <summary>
    /// Runs the shared approval-timeout pipeline for every supported request module.
    /// The worker interval is intentionally shorter than the escalation threshold so
    /// a request is not left waiting for hours after its rule has expired.
    /// </summary>
    public class EscalationBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EscalationBackgroundWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(15);

        public EscalationBackgroundWorker(
            IServiceProvider serviceProvider,
            ILogger<EscalationBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "[ESC_WORKER] Started, interval={Minutes}m",
                _period.TotalMinutes);

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
                // App đang shutdown.
            }

            _logger.LogInformation("[ESC_WORKER] Stopped");
        }

        private async Task DoWorkAsync(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("[ESC_WORKER] Running at {Time}", DateTimeOffset.Now);

                await using var scope = _serviceProvider.CreateAsyncScope();

                var leaveEscalation = scope.ServiceProvider.GetRequiredService<ILeaveEscalationService>();
                await leaveEscalation.ProcessAsync(ct);

                var otEscalation = scope.ServiceProvider.GetRequiredService<IOTEscalationService>();
                await otEscalation.ProcessAsync(ct);

                _logger.LogInformation("[ESC_WORKER] Completed at {Time}", DateTimeOffset.Now);
            }
            catch (OperationCanceledException)
            {
                // Bỏ qua khi shutdown.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ESC_WORKER] Error during execution");
            }
        }
    }
}

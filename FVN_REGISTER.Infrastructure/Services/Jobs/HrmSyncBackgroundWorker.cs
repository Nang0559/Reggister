using FVN_REGISTER.Application.Interfaces.HrmSync;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Jobs
{
    public sealed class HrmSyncBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HrmSyncBackgroundWorker> _logger;

        public HrmSyncBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<HrmSyncBackgroundWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(DelayUntilNextRun(DateTime.Now, 2, 0), stoppingToken);
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var sync = scope.ServiceProvider.GetRequiredService<IHrmSyncService>();
                    var result = await sync.RunAllAsync("SYSTEM", manual: false, stoppingToken);

                    if (!result.IsSuccess || result.Data is not { } run || !run.Success)
                        _logger.LogError("[HRM-SYNC] Daily synchronization failed: {Message}", result.Message ?? run?.Summary);

                    else
                        _logger.LogInformation("[HRM-SYNC] Daily synchronization completed: {Summary}", run.Summary);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[HRM-SYNC] Daily synchronization failed.");
                }
            }
        }

        private static TimeSpan DelayUntilNextRun(DateTime now, int hour, int minute)
        {
            var next = now.Date.AddHours(hour).AddMinutes(minute);
            if (next <= now) next = next.AddDays(1);
            return next - now;
        }
    }
}

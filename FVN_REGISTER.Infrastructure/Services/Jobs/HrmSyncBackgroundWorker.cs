using FVN_REGISTER.Application.Interfaces.HrmSync;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Jobs
{
    /// <summary>
    /// Daily HRM synchronization pipeline:
    /// Source Reader -> Staging Importer -> Staging -> Sync Job -> Domain tables.
    /// The worker only orchestrates Application ports; all I/O remains in Infrastructure adapters.
    /// </summary>
    public sealed class HrmSyncBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HrmSyncBackgroundWorker> _logger;

        public HrmSyncBackgroundWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<HrmSyncBackgroundWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = DelayUntilNextRun(DateTime.Now, hour: 2, minute: 0);
                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    await RunOnceAsync(DateTime.Today, stoppingToken);
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

        private async Task RunOnceAsync(DateTime asOfDate, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var importers = scope.ServiceProvider.GetRequiredService<IHrmStagingImporterResolver>().GetAll();
            var jobs = scope.ServiceProvider.GetRequiredService<IHrmSyncJobResolver>().GetAll()
                .OrderBy(x => x.SyncOrder)
                .ToList();

            foreach (var importer in importers)
            {
                ct.ThrowIfCancellationRequested();
                var count = await importer.ImportAsync(asOfDate, ct);
                _logger.LogInformation("[HRM-SYNC] Imported {Count} rows for {EntityType}.", count, importer.EntityType);
            }

            foreach (var job in jobs)
            {
                ct.ThrowIfCancellationRequested();
                var result = await job.RunAsync(ct);
                _logger.LogInformation(
                    "[HRM-SYNC] {EntityType}: Success={Success} Message={Message}",
                    job.EntityType, result.Success, result.Message);

                if (!result.Success && job.IsBlockingDependency)
                    throw new InvalidOperationException(
                        $"HRM sync job '{job.EntityType}' failed and is a blocking dependency: {result.Message}");
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

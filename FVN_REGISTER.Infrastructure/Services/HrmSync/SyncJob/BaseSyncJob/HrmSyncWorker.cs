using FVN_REGISTER.Application.Interfaces.HrmSync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.BaseSyncJob
{
    public class HrmSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HrmSyncWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromSeconds(30);

        public HrmSyncWorker(IServiceProvider serviceProvider, ILogger<HrmSyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HRM Sync Worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var resolver = scope.ServiceProvider.GetRequiredService<IHrmSyncJobResolver>();

                    foreach (var job in resolver.GetAll())
                    {
                        try
                        {
                            var result = await job.RunAsync(stoppingToken);
                            if (result.TotalSource > 0)
                            {
                                _logger.LogInformation("[HRM_SYNC:{Type}] {Summary}",
                                    job.EntityType, result.Summary);
                            }
                            if (!result.Success && job.IsBlockingDependency)
                            {
                                _logger.LogWarning(
                                    "[HRM_SYNC:{Type}] Blocking dependency thất bại ({ErrorCount} lỗi) — " +
                                    "dừng chuỗi sync các job còn lại trong vòng lặp này để tránh vỡ FK.",
                                    job.EntityType, result.Errors.Count);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[HRM_SYNC:{Type}] Job failed", job.EntityType);
                            if (job.IsBlockingDependency)
                            {
                                _logger.LogWarning(
                                    "[HRM_SYNC:{Type}] Exception ở blocking dependency — dừng chuỗi.",
                                    job.EntityType);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "HRM Sync Worker loop error");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("HRM Sync Worker stopped");
        }
    }
}

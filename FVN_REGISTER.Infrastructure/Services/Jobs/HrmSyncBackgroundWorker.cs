using FVN_REGISTER.Application.Interfaces.HrmSync;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Jobs
{
    public sealed class HrmSyncBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HrmSyncBackgroundWorker> _logger;
        private readonly IConfiguration _configuration;

        private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromMinutes(5);

        public HrmSyncBackgroundWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<HrmSyncBackgroundWorker> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        private TimeSpan GetPollInterval()
        {
            var minutes = _configuration.GetValue<int?>("HrmSync:PollMinutes");
            return minutes is > 0 and <= 1440
                ? TimeSpan.FromMinutes(minutes.Value)
                : DefaultPollInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Polling thay cho trigger cross-database: HRM không bị phụ thuộc vào FVN_REGISTER.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var sync = scope.ServiceProvider.GetRequiredService<IHrmSyncService>();
                    var result = await sync.RunAllAsync("SYSTEM", manual: false, stoppingToken);
                    var run = result.Data;

                    if (!result.IsSuccess || run == null || !run.Success)
                    {
                        _logger.LogError(
                            "[HRM-SYNC] Automatic synchronization failed: {Message}",
                            result.Message ?? run?.Summary);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "[HRM-SYNC] Automatic synchronization completed: {Summary}",
                            run.Summary);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // HRM/FVN failure must not terminate the hosted worker.
                    _logger.LogError(ex, "[HRM-SYNC] Automatic synchronization failed.");
                }

                try
                {
                    await Task.Delay(GetPollInterval(), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}

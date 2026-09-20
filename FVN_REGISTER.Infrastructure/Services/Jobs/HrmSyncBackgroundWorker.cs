using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Interfaces.Leaves;
using Microsoft.Extensions.Configuration;
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
            // Không chạy full HRM sync ngay khi API vừa khởi động:
            // sync là tác vụ nền nặng và không được cạnh tranh tài nguyên với
            // authentication/dashboard của người dùng đầu tiên.
            var runOnStartup = _configuration.GetValue<bool>("HrmSync:RunOnStartup");
            if (runOnStartup)
            {
                var startupDelaySeconds = _configuration.GetValue<int?>("HrmSync:StartupDelaySeconds") ?? 30;
                if (startupDelaySeconds > 0)
                {
                    try
                    {
                        await Task.Delay(
                            TimeSpan.FromSeconds(Math.Min(startupDelaySeconds, 3600)),
                            stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
            else
            {
                // Mặc định: nhường toàn bộ startup path cho người dùng,
                // chạy lần đầu sau đúng một chu kỳ polling.
                try
                {
                    await Task.Delay(GetPollInterval(), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
            }

            // Polling thay cho trigger cross-database: HRM không bị phụ thuộc vào FVN_REGISTER.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var sync = scope.ServiceProvider.GetRequiredService<IHrmSyncService>();
                    var result = await sync.RunAllAsync("SYSTEM", manual: false, stoppingToken);
                    var run = result.Data;

                    // HRM sync tạo/cập nhật Employee trước; entitlement chạy ngay sau đó
                    // để nhân viên mới có balance của năm hiện tại mà không cần mở màn hình
                    // "Phép năm". Phương thức này idempotent nên worker có thể chạy mỗi chu kỳ.
                    if (result.IsSuccess)
                    {
                        var entitlement = scope.ServiceProvider.GetRequiredService<ILeaveEntitlementService>();
                        await entitlement.EnsureWorkYearCalculatedAsync(DateTime.Today.Year, stoppingToken);
                    }

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

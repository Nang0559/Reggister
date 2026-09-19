using FVN_REGISTER.Application.Interfaces.Jobs;
using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Dtos.OtReasons;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace FVN_REGISTER.Infrastructure.Services.Jobs
{
    /// <summary>
    /// Chạy mỗi đêm HRM-compatible calculation để tạo F03HrmAttendanceCalculated/F03HrmOTActual.
    /// Worker không ghi vào HRM và không chạy bước Reconcile/Approve OT.
    /// </summary>
    public class OTAttendanceStagingWorker : BackgroundService, IOTWorkerStatus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OTAttendanceStagingWorker> _logger;

#if DEBUG
        private readonly TimeSpan _runAt = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(2));
#else
        private readonly TimeSpan _runAt = new TimeSpan(0, 30, 0);
#endif

        private WorkerRunState _state = WorkerRunState.Waiting;
        private DateTime? _nextRunAt;
        private DateTime? _lastRunAt;
        private DateTime? _lastFinished;
        private string? _lastResult;
        private string? _lastMessage;
        private int? _lastCount;

        public OTAttendanceStagingWorker(
            IServiceProvider serviceProvider,
            ILogger<OTAttendanceStagingWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public OTWorkerStatusDto GetStatus() => new()
        {
            State = _state,
            StateText = _state.ToDisplayName(),
            NextRunAt = _nextRunAt,
            LastRunAt = _lastRunAt,
            LastRunFinishedAt = _lastFinished,
            LastRunResult = _lastResult,
            LastRunMessage = _lastMessage,
            LastSyncedCount = _lastCount,
            ServerTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
        };

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[OT_STAGING_WORKER] Khởi động.");
            _state = WorkerRunState.Waiting;

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = DateTime.Today.Add(_runAt);
                if (now > nextRun) nextRun = nextRun.AddDays(1);

                _nextRunAt = nextRun;
                _state = WorkerRunState.Waiting;

                var delay = nextRun - now;
                _logger.LogInformation(
                    "[OT_STAGING_WORKER] Chờ đến {NextRun} ({Hours:F1}h nữa)",
                    nextRun.ToString("dd/MM/yyyy HH:mm"), delay.TotalHours);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await RunStagingSyncAsync(stoppingToken);
            }

            _state = WorkerRunState.Stopped;
            _logger.LogInformation("[OT_STAGING_WORKER] Dừng.");
        }

        private async Task RunStagingSyncAsync(CancellationToken ct)
        {
            _state = WorkerRunState.Running;
            _lastRunAt = DateTime.Now;
            _lastResult = null;
            _lastMessage = null;
            _lastCount = null;

            _logger.LogInformation("[OT_STAGING_WORKER] Bắt đầu sync lúc {T}", DateTime.Now.ToString("HH:mm:ss"));

            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();

                var calculation = scope.ServiceProvider.GetRequiredService<IHrmAttendanceCalculationService>();

                var yesterday = DateTime.Today.AddDays(-1);
                var result = await calculation.CalculateAsync(
                    new FVN_REGISTER.Contract.Dtos.HrmSync.HrmAttendanceCalculationRequestDto
                    {
                        DeptCode = null,
                        FromDate = yesterday,
                        ToDate = yesterday
                    },
                    "OT-STAGING-WORKER",
                    ct);

                var count = result.IsSuccess && result.Data is not null ? result.Data.CalculatedRows : 0;

                _lastFinished = DateTime.Now;
                _lastResult = result.IsSuccess ? "OK" : "FAILED";
                _lastMessage = result.IsSuccess
                    ? $"Đã tính HRM-compatible {count} dòng chấm công ngày {yesterday:dd/MM/yyyy}. F03HrmOTActual đã được cập nhật cho đối chiếu OT."
                    : (result.Message ?? "HRM-compatible calculation failed.");
                _lastCount = count;
                _state = WorkerRunState.Waiting;

                if (count == 0)
                {
                    _logger.LogWarning(
                        "[OT_STAGING_WORKER] 0 bản ghi ngày {D} — kiểm tra dữ liệu chấm công HRM hoặc đơn OT đã duyệt",
                        yesterday.ToString("dd/MM/yyyy"));
                }
                else
                {
                    _logger.LogInformation(
                        "[OT_STAGING_WORKER] Sync OK ngày {D}: {Count} bản ghi",
                        yesterday.ToString("dd/MM/yyyy"), count);
                }
            }
            catch (OperationCanceledException)
            {
                _state = WorkerRunState.Stopped;
                _lastResult = "CANCELLED";
                _logger.LogWarning("[OT_STAGING_WORKER] Sync bị hủy.");
            }
            catch (Exception ex)
            {
                _state = WorkerRunState.Error;
                _lastResult = "FAILED";
                _lastMessage = ex.Message;
                _lastFinished = DateTime.Now;
                _logger.LogError(ex, "[OT_STAGING_WORKER] Lỗi không mong muốn");
            }
        }
    }
}

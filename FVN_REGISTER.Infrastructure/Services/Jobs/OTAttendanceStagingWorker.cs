using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Contract.Dtos.OtReasons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace FVN_REGISTER.Infrastructure.Services.Jobs
{
    /// <summary>
    /// Chạy mỗi đêm HRM-compatible calculation để tạo F03HrmAttendanceCalculated/F03HrmOTActual.
    /// Worker không ghi vào HRM và không chạy bước Reconcile/Approve OT.
    /// </summary>
    public class OTAttendanceStagingWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OTAttendanceStagingWorker> _logger;

#if DEBUG
        private readonly TimeSpan _runAt = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(2));
#else
        private readonly TimeSpan _runAt = new TimeSpan(0, 30, 0);
#endif


        public OTAttendanceStagingWorker(
            IServiceProvider serviceProvider,
            ILogger<OTAttendanceStagingWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[HRM_ATTENDANCE_WORKER] Khởi động.");
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = DateTime.Today.Add(_runAt);
                if (now > nextRun) nextRun = nextRun.AddDays(1);

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

            _logger.LogInformation("[OT_STAGING_WORKER] Dừng.");
        }

        private async Task RunStagingSyncAsync(CancellationToken ct)
        {
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
                _logger.LogWarning("[HRM_ATTENDANCE_WORKER] Calculation bị hủy.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HRM_ATTENDANCE_WORKER] Lỗi không mong muốn");
            }
        }
    }
}

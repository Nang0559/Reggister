using FVN_REGISTER.Application.Interfaces.HrmSync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Jobs;

/// <summary>
/// Chạy định kỳ HRM-compatible attendance calculation.
/// Đây là pipeline nền duy nhất tính chấm công và cập nhật actual OT;
/// OT/Leave/Trip không có worker đồng bộ riêng.
/// </summary>
public sealed class OTAttendanceStagingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OTAttendanceStagingWorker> _logger;

#if DEBUG
    private readonly TimeSpan _runAt = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(2));
#else
    private readonly TimeSpan _runAt = new(0, 30, 0);
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
            if (now > nextRun)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            _logger.LogInformation(
                "[HRM_ATTENDANCE_WORKER] Chờ đến {NextRun} ({Hours:F1}h nữa)",
                nextRun.ToString("dd/MM/yyyy HH:mm"),
                delay.TotalHours);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await RunCalculationAsync(stoppingToken);
        }

        _logger.LogInformation("[HRM_ATTENDANCE_WORKER] Dừng.");
    }

    private async Task RunCalculationAsync(CancellationToken ct)
    {
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
                "HRM-ATTENDANCE-WORKER",
                ct);

            if (result.IsSuccess && result.Data is not null)
            {
                _logger.LogInformation(
                    "[HRM_ATTENDANCE_WORKER] Calculation OK ngày {Date}: {Count} dòng.",
                    yesterday.ToString("dd/MM/yyyy"),
                    result.Data.CalculatedRows);
            }
            else
            {
                _logger.LogWarning(
                    "[HRM_ATTENDANCE_WORKER] Calculation failed ngày {Date}: {Message}",
                    yesterday.ToString("dd/MM/yyyy"),
                    result.Message ?? "Unknown error");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[HRM_ATTENDANCE_WORKER] Calculation bị hủy.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HRM_ATTENDANCE_WORKER] Lỗi không mong muốn.");
        }
    }
}

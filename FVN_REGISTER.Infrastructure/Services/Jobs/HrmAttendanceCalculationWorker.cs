using FVN_REGISTER.Application.Interfaces.HrmSync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Jobs;

/// <summary>
/// Company-wide attendance calculation scheduler.
///
/// Contract:
/// 1. On application start, backfill the current payroll period through yesterday.
/// 2. Every day at 00:30, calculate yesterday for the whole company.
/// 3. Manual department/date calculation uses the same IHrmAttendanceCalculationService
///    and the same dbo.usp_CalculateHrmAttendance procedure.
/// </summary>
public sealed class HrmAttendanceCalculationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HrmAttendanceCalculationWorker> _logger;

    private static readonly TimeSpan DailyRunAt = new(0, 30, 0);

    public HrmAttendanceCalculationWorker(
        IServiceProvider serviceProvider,
        ILogger<HrmAttendanceCalculationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[HRM_ATTENDANCE_WORKER] Khởi động.");

        // Never wait for the first scheduled run when the application starts.
        // This closes the gap after deployment/restart and guarantees the current
        // payroll period has calculated data before users open the dashboard.
        await RunCatchUpAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = DateTime.Today.Add(DailyRunAt);
            if (now >= nextRun)
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

            await RunYesterdayAsync(stoppingToken);
        }

        _logger.LogInformation("[HRM_ATTENDANCE_WORKER] Dừng.");
    }

    private async Task RunCatchUpAsync(CancellationToken ct)
    {
        var yesterday = DateTime.Today.AddDays(-1).Date;
        var payrollStart = GetPayrollPeriodStart(yesterday);

        _logger.LogInformation(
            "[HRM_ATTENDANCE_WORKER] Catch-up company-wide: {From} -> {To}",
            payrollStart.ToString("dd/MM/yyyy"),
            yesterday.ToString("dd/MM/yyyy"));

        await CalculateCompanyWideAsync(payrollStart, yesterday, "HRM-ATTENDANCE-WORKER-CATCHUP", ct);
    }

    private async Task RunYesterdayAsync(CancellationToken ct)
    {
        var yesterday = DateTime.Today.AddDays(-1).Date;

        _logger.LogInformation(
            "[HRM_ATTENDANCE_WORKER] Daily company-wide calculation: {Date}",
            yesterday.ToString("dd/MM/yyyy"));

        await CalculateCompanyWideAsync(
            yesterday,
            yesterday,
            "HRM-ATTENDANCE-WORKER",
            ct);
    }

    private async Task CalculateCompanyWideAsync(
        DateTime from,
        DateTime to,
        string triggeredBy,
        CancellationToken ct)
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var calculation = scope.ServiceProvider.GetRequiredService<IHrmAttendanceCalculationService>();

            var result = await calculation.CalculateAsync(
                new FVN_REGISTER.Contract.Dtos.HrmSync.HrmAttendanceCalculationRequestDto
                {
                    // NULL = all active HRM employees/company-wide.
                    DeptCode = null,
                    FromDate = from,
                    ToDate = to
                },
                triggeredBy,
                ct);

            if (result.IsSuccess && result.Data is not null)
            {
                _logger.LogInformation(
                    "[HRM_ATTENDANCE_WORKER] Calculation OK {From} -> {To}: {Employees} nhân viên / {Rows} dòng. Batch={BatchId}",
                    from.ToString("dd/MM/yyyy"),
                    to.ToString("dd/MM/yyyy"),
                    result.Data.EmployeeCount,
                    result.Data.CalculatedRows,
                    result.Data.CalculationBatchId);
            }
            else
            {
                _logger.LogWarning(
                    "[HRM_ATTENDANCE_WORKER] Calculation failed {From} -> {To}: {Message}",
                    from.ToString("dd/MM/yyyy"),
                    to.ToString("dd/MM/yyyy"),
                    result.Message ?? "Unknown error");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "[HRM_ATTENDANCE_WORKER] Calculation bị hủy {From} -> {To}.",
                from.ToString("dd/MM/yyyy"),
                to.ToString("dd/MM/yyyy"));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[HRM_ATTENDANCE_WORKER] Lỗi khi tính {From} -> {To}.",
                from.ToString("dd/MM/yyyy"),
                to.ToString("dd/MM/yyyy"));
        }
    }

    private static DateTime GetPayrollPeriodStart(DateTime date)
    {
        // Payroll period: 21st of current month -> 20th of next month.
        // For a date on/before the 20th, the period started on the 21st
        // of the previous month.
        return date.Day >= 21
            ? new DateTime(date.Year, date.Month, 21)
            : new DateTime(date.Year, date.Month, 1).AddMonths(-1).Date.AddDays(20);
    }
}
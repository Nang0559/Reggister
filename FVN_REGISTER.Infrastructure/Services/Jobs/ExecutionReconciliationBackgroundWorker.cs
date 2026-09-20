using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Execution;
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Jobs;

public sealed class ExecutionReconciliationBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExecutionReconciliationBackgroundWorker> _logger;

    public ExecutionReconciliationBackgroundWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ExecutionReconciliationBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                await RunOnceAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution reconciliation worker failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private async Task RunOnceAsync(IServiceProvider services, CancellationToken ct)
    {
        var db = services.GetRequiredService<FVNWEBAPPContext>();
        var reconciliation = services.GetRequiredService<IExecutionReconciliationService>();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var from = today.AddDays(-2);
        var to = today;

        var policies = await db.ExecutionPolicies.AsNoTracking()
            .Where(x => x.IsActive != false && x.ReconciliationMode != 0)
            .ToDictionaryAsync(x => x.ModuleCode, StringComparer.OrdinalIgnoreCase, ct);

        if (policies.ContainsKey("OT"))
            await ReconcileOtAsync(db, reconciliation, from, to, ct);

        if (policies.ContainsKey("LEAVE"))
            await ReconcileLeaveAsync(db, reconciliation, from, to, ct);

        if (policies.ContainsKey("TRIP"))
            await ReconcileTripAsync(db, reconciliation, from, to, ct);
    }

    private async Task ReconcileOtAsync(
        FVNWEBAPPContext db,
        IExecutionReconciliationService service,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        var rows = await db.OvertimeEmployees.AsNoTracking()
            .Where(x => x.OTRequest.RequestStatus == ApprovalStatus.Approved
                && x.OTRequest.IsActive != false
                && x.OTRequest.OTDate >= from.ToDateTime(TimeOnly.MinValue)
                && x.OTRequest.OTDate <= to.ToDateTime(TimeOnly.MaxValue))
            .Select(x => new
            {
                x.EmployeeCode,
                x.OTRequestId,
                x.OTRequest.OTCode,
                x.OTRequest.OTDate,
                x.OTHours,
                x.ActualHours,
                x.ActualStartTime,
                x.ActualEndTime
            })
            .ToListAsync(ct);

        foreach (var row in rows)
        {
            try
            {
                var workDate = DateOnly.FromDateTime(row.OTDate);
                var actual = row.ActualHours;
                var planned = row.OTHours;

                // Today's OT is still open until the actual-hours feed closes.
                var status = actual.HasValue
                    ? Math.Abs(actual.Value - planned) <= 0.1m ? "Matched" : "Mismatch"
                    : workDate < DateOnly.FromDateTime(DateTime.Today) ? "Mismatch" : "None";

                await service.UpsertAsync(
                    row.EmployeeCode,
                    new ExecutionReconciliationUpsertRequest(
                        "OT",
                        "OT_EMPLOYEE",
                        $"{row.OTCode}:{row.EmployeeCode}",
                        row.EmployeeCode,
                        await ResolveEmployeeIdAsync(db, row.EmployeeCode, ct),
                        workDate,
                        $"ApprovedHours={planned:0.##}",
                        actual.HasValue ? $"ActualHours={actual.Value:0.##}" : "NO_ACTUAL",
                        status,
                        status == "Mismatch",
                        status == "Mismatch",
                        JsonSerializer.Serialize(new
                        {
                            row.OTRequestId,
                            row.OTCode,
                            row.EmployeeCode,
                            PlannedHours = planned,
                            ActualHours = actual,
                            row.ActualStartTime,
                            row.ActualEndTime
                        })),
                    ct,
                    actorUserId: 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Execution reconciliation skipped OT {EmployeeCode}/{OTRequestId}.", row.EmployeeCode, row.OTRequestId);
            }
            finally
            {
                db.ChangeTracker.Clear();
            }
        }
    }

    private async Task ReconcileLeaveAsync(
        FVNWEBAPPContext db,
        IExecutionReconciliationService service,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        var rows = await db.VF03LeaveRequests.AsNoTracking()
            .Where(x => x.IsActive == true
                && x.RequestStatus == ApprovalStatus.Approved
                && x.EndDate >= from.ToDateTime(TimeOnly.MinValue)
                && x.StartDate <= to.ToDateTime(TimeOnly.MaxValue))
            .Select(x => new
            {
                x.Id,
                x.LeaveCode,
                x.EmployeeCode,
                x.StartDate,
                x.EndDate,
                x.LeaveTypeCode,
                x.LeaveTypeName
            })
            .ToListAsync(ct);

        foreach (var row in rows)
        {
            var start = DateOnly.FromDateTime(row.StartDate < from.ToDateTime(TimeOnly.MinValue)
                ? from.ToDateTime(TimeOnly.MinValue)
                : row.StartDate);
            var end = DateOnly.FromDateTime(row.EndDate > to.ToDateTime(TimeOnly.MaxValue)
                ? to.ToDateTime(TimeOnly.MaxValue)
                : row.EndDate);

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                try
                {
                    var employeeId = await ResolveEmployeeIdAsync(db, row.EmployeeCode, ct);

                    var attendance = await db.VF03EmployeeAttendances.AsNoTracking()
                        .Where(x => x.EmployeeId == row.EmployeeCode && x.Date == date)
                        .Select(x => new { x.CheckInTime, x.CheckOutTime })
                        .FirstOrDefaultAsync(ct);

                    var worked = attendance?.CheckInTime.HasValue == true
                        || attendance?.CheckOutTime.HasValue == true;

                    var status = worked ? "Mismatch" : "Matched";

                    await service.UpsertAsync(
                        row.EmployeeCode,
                        new ExecutionReconciliationUpsertRequest(
                            "LEAVE",
                            "LEAVE_DAY",
                            $"{row.Id}:{date:yyyyMMdd}",
                            null,
                            employeeId,
                            date,
                            $"ApprovedLeave={row.LeaveTypeCode ?? row.LeaveTypeName ?? "LEAVE"}",
                            worked ? "WORKED" : "ABSENT",
                            status,
                            status == "Mismatch",
                            status == "Mismatch",
                            JsonSerializer.Serialize(new
                            {
                                row.Id,
                                row.LeaveCode,
                                row.EmployeeCode,
                                row.LeaveTypeCode,
                                WorkDate = date,
                                Attendance = attendance
                            })),
                        ct,
                        actorUserId: 0);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Execution reconciliation skipped Leave {EmployeeCode}/{Date}.", row.EmployeeCode, date);
                }
                finally
                {
                    db.ChangeTracker.Clear();
                }
            }
        }
    }

    private async Task ReconcileTripAsync(
        FVNWEBAPPContext db,
        IExecutionReconciliationService service,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        var requests = await db.TripRequests.AsNoTracking()
            .Where(x => x.IsActive == true
                && x.RequestStatus == ApprovalStatus.Approved
                && x.EndDate >= from.ToDateTime(TimeOnly.MinValue)
                && x.StartDate <= to.ToDateTime(TimeOnly.MaxValue))
            .Select(x => new
            {
                x.Id,
                x.TripCode,
                x.EmployeeCode,
                x.StartDate,
                x.EndDate,
                x.Destination,
                x.Purpose
            })
            .ToListAsync(ct);

        foreach (var row in requests)
        {
            try
            {
                var employeeId = await ResolveEmployeeIdAsync(db, row.EmployeeCode, ct);
                var actualRows = await db.Database.SqlQuery<TripActualWorkerRow>(
                    $"SELECT TripRequestId, EmployeeCode, ActualStartDate, ActualEndDate, Status FROM dbo.F03TripActual WHERE TripRequestId = {row.Id}")
                    .ToListAsync(ct);

                var actual = actualRows.FirstOrDefault();
                var today = DateOnly.FromDateTime(DateTime.Today);
                var endDate = DateOnly.FromDateTime(row.EndDate);
                var workDate = DateOnly.FromDateTime(row.StartDate);

                // F03TripActual is created as Scheduled at approval time.
                // It is not evidence of completion. Only Completed after EndDate is Matched.
                var status = actual is null
                    ? endDate < today ? "Mismatch" : "None"
                    : string.Equals(actual.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                        ? "Matched"
                        : endDate < today ? "Mismatch" : "None";

                await service.UpsertAsync(
                    row.EmployeeCode,
                    new ExecutionReconciliationUpsertRequest(
                        "TRIP",
                        "TRIP_REQUEST",
                        row.TripCode,
                        null,
                        employeeId,
                        workDate,
                        "Approved",
                        actual is null ? "NO_ACTUAL" : actual.Status ?? "EXECUTED",
                        status,
                        status == "Mismatch",
                        status == "Mismatch",
                        JsonSerializer.Serialize(new
                        {
                            row.Id,
                            row.TripCode,
                            row.EmployeeCode,
                            row.StartDate,
                            row.EndDate,
                            row.Destination,
                            row.Purpose,
                            Actual = actual
                        })),
                    ct,
                    actorUserId: 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Execution reconciliation skipped Trip {EmployeeCode}/{TripRequestId}.", row.EmployeeCode, row.Id);
            }
            finally
            {
                db.ChangeTracker.Clear();
            }
        }
    }

    private static Task<int> ResolveEmployeeIdAsync(
        FVNWEBAPPContext db,
        string employeeCode,
        CancellationToken ct) =>
        db.Employees.AsNoTracking()
            .Where(x => x.IsActive != false && x.EmployeeCode == employeeCode)
            .Select(x => x.Id)
            .SingleAsync(ct);

    private sealed class TripActualWorkerRow
    {
        public int TripRequestId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string? Status { get; set; }
    }
}

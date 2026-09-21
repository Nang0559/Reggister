using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Execution;
using FVN_REGISTER.Contract.Dtos.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        if (policies.TryGetValue("OT", out var otPolicy))
            await ReconcileOtAsync(db, reconciliation, from, to, otPolicy.ReconciliationMode, ct);

        if (policies.TryGetValue("LEAVE", out var leavePolicy))
            await ReconcileLeaveAsync(db, reconciliation, from, to, leavePolicy.ReconciliationMode, ct);

        if (policies.TryGetValue("TRIP", out var tripPolicy))
            await ReconcileTripAsync(db, reconciliation, from, to, tripPolicy.ReconciliationMode, ct);
    }

    private static async Task<HashSet<string>> GetUnresolvedSourceIdsAsync(
        FVNWEBAPPContext db,
        string moduleCode,
        CancellationToken ct)
    {
        return (await db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.IsActive != false
                && x.ModuleCode == moduleCode
                && (x.ReconciliationStatus == "Mismatch"
                    || x.ReconciliationStatus == "AwaitingConfirmation"))
            .Select(x => x.SourceId)
            .Distinct()
            .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task ReconcileOtAsync(
        FVNWEBAPPContext db,
        IExecutionReconciliationService service,
        DateOnly from,
        DateOnly to,
        byte reconciliationMode,
        CancellationToken ct)
    {
        var unresolved = reconciliationMode == 2
            ? await GetUnresolvedSourceIdsAsync(db, "OT", ct)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rows = await db.OvertimeEmployees.AsNoTracking()
            .Where(x => (x.OTRequest.RequestStatus == ApprovalStatus.Approved
                         || x.OTRequest.RequestStatus == ApprovalStatus.Cancelled)
                && x.OTRequest.IsActive != false
                && (x.OTRequest.OTDate >= from.ToDateTime(TimeOnly.MinValue)
                    && x.OTRequest.OTDate <= to.ToDateTime(TimeOnly.MaxValue)
                    || unresolved.Contains(x.OTRequest.OTCode + ":" + x.EmployeeCode)))
            .Select(x => new
            {
                x.EmployeeCode,
                x.OTRequestId,
                x.OTRequest.OTCode,
                x.OTRequest.OTDate,
                x.OTRequest.RequestStatus,
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
                var cancelled = row.RequestStatus == ApprovalStatus.Cancelled;
                if (cancelled)
                {
                    var cancelledEmployeeId = await ResolveEmployeeIdAsync(db, row.EmployeeCode, ct);
                    var existing = await db.ExecutionReconciliations.AsNoTracking()
                        .AnyAsync(x => x.IsActive != false
                            && x.ModuleCode == "OT"
                            && x.SourceType == "OT_EMPLOYEE"
                            && x.SourceId == $"{row.OTCode}:{row.EmployeeCode}"
                            && x.EmployeeId == cancelledEmployeeId
                            && x.WorkDate == workDate, ct);

                    if (!existing)
                        continue;
                }

                var actual = row.ActualHours;
                var planned = row.OTHours;

                var status = cancelled
                    ? "Resolved"
                    : actual.HasValue
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
                        cancelled ? "CANCELLED" : $"ApprovedHours={planned:0.##}",
                        cancelled
                            ? "CANCELLED"
                            : actual.HasValue ? $"ActualHours={actual.Value:0.##}" : "NO_ACTUAL",
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
                            row.ActualEndTime,
                            row.RequestStatus
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
        byte reconciliationMode,
        CancellationToken ct)
    {
        var unresolved = reconciliationMode == 2
            ? await GetUnresolvedSourceIdsAsync(db, "LEAVE", ct)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var unresolvedLeaveIds = unresolved
            .Select(x => x.Split(':', 2)[0])
            .Select(x => int.TryParse(x, out var id) ? (int?)id : null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        var rows = await db.VF03LeaveRequests.AsNoTracking()
            .Where(x => x.IsActive == true
                && (x.RequestStatus == ApprovalStatus.Approved
                    || x.RequestStatus == ApprovalStatus.Cancelled)
                && (x.EndDate >= from.ToDateTime(TimeOnly.MinValue)
                    && x.StartDate <= to.ToDateTime(TimeOnly.MaxValue)
                    || unresolvedLeaveIds.Contains(x.Id)))
            .Select(x => new
            {
                x.Id,
                x.EmployeeCode,
                x.StartDate,
                x.EndDate,
                x.LeaveTypeCode,
                x.LeaveTypeName,
                x.RequestStatus
            })
            .ToListAsync(ct);

        var leaveIds = rows.Select(x => x.Id).Distinct().ToList();
        var details = await db.Set<F03LeaveDayDetail>().AsNoTracking()
            .Where(x => leaveIds.Contains(x.LeaveDaysId)
                && x.IsActive != false
                && x.IsCountedAsLeave)
            .ToListAsync(ct);

        foreach (var row in rows)
        {
            var rowDetails = details
                .Where(x => x.LeaveDaysId == row.Id)
                .OrderBy(x => x.LeaveDate)
                .ToList();

            // F03LeaveDayDetails is the source for counted leave days.
            // Do not synthesize weekends, holidays or non-counted dates.
            foreach (var detail in rowDetails)
            {
                var date = DateOnly.FromDateTime(detail.LeaveDate);
                if (date < from || date > to)
                {
                    var sourceId = $"{row.Id}:{date:yyyyMMdd}";
                    if (!unresolved.Contains(sourceId))
                        continue;
                }

                try
                {
                    var employeeId = await ResolveEmployeeIdAsync(db, row.EmployeeCode, ct);
                    var cancelled = row.RequestStatus == ApprovalStatus.Cancelled;

                    if (cancelled)
                    {
                        var existing = await db.ExecutionReconciliations.AsNoTracking()
                            .AnyAsync(x => x.IsActive != false
                                && x.ModuleCode == "LEAVE"
                                && x.SourceType == "LEAVE_DAY"
                                && x.SourceId == $"{row.Id}:{date:yyyyMMdd}"
                                && x.EmployeeId == employeeId
                                && x.WorkDate == date, ct);

                        if (!existing)
                            continue;
                    }

                    var attendance = await db.VF03EmployeeAttendances.AsNoTracking()
                        .Where(x => x.EmployeeId == row.EmployeeCode && x.Date == date)
                        .Select(x => new { x.CheckInTime, x.CheckOutTime })
                        .FirstOrDefaultAsync(ct);

                    var worked = attendance?.CheckInTime.HasValue == true
                        || attendance?.CheckOutTime.HasValue == true;

                    // Full-day leave with any attendance is a mismatch.
                    // Half-day leave is retained as planned metadata; a punch alone
                    // is not treated as a mismatch because the employee is expected
                    // to work part of that day.
                    var mismatch = !detail.IsHalfDay && worked;
                    var status = cancelled ? "Resolved" : mismatch ? "Mismatch" : "Matched";

                    await service.UpsertAsync(
                        row.EmployeeCode,
                        new ExecutionReconciliationUpsertRequest(
                            "LEAVE",
                            "LEAVE_DAY",
                            $"{row.Id}:{date:yyyyMMdd}",
                            null,
                            employeeId,
                            date,
                            cancelled
                                ? "CANCELLED"
                                : $"ApprovedLeave={detail.LeaveTypeCode};DayValue={detail.DayValue:0.##};HalfDay={detail.IsHalfDay}",
                            cancelled
                                ? "CANCELLED"
                                : worked ? "WORKED" : "ABSENT",
                            status,
                            status == "Mismatch",
                            status == "Mismatch",
                            JsonSerializer.Serialize(new
                            {
                                row.Id,
                                row.EmployeeCode,
                                detail.LeaveTypeCode,
                                detail.IsCountedAsLeave,
                                detail.IsHalfDay,
                                detail.DayValue,
                                WorkDate = date,
                                Attendance = attendance,
                                row.RequestStatus
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
        byte reconciliationMode,
        CancellationToken ct)
    {
        var unresolved = reconciliationMode == 2
            ? await GetUnresolvedSourceIdsAsync(db, "TRIP", ct)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var requests = await db.TripRequests.AsNoTracking()
            .Where(x => x.IsActive == true
                && (x.RequestStatus == ApprovalStatus.Approved
                    || x.RequestStatus == ApprovalStatus.Cancelled)
                && (x.EndDate >= from.ToDateTime(TimeOnly.MinValue)
                    && x.StartDate <= to.ToDateTime(TimeOnly.MaxValue)
                    || unresolved.Contains(x.TripCode)))
            .Select(x => new
            {
                x.Id,
                x.TripCode,
                x.EmployeeCode,
                x.StartDate,
                x.EndDate,
                x.Destination,
                x.Purpose,
                x.RequestStatus
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
                var plannedStart = DateOnly.FromDateTime(row.StartDate);
                var plannedEnd = DateOnly.FromDateTime(row.EndDate);
                var workDate = plannedStart;

                var cancelled = row.RequestStatus == ApprovalStatus.Cancelled;

                if (cancelled)
                {
                    var existing = await db.ExecutionReconciliations.AsNoTracking()
                        .AnyAsync(x => x.IsActive != false
                            && x.ModuleCode == "TRIP"
                            && x.SourceType == "TRIP_REQUEST"
                            && x.SourceId == row.TripCode
                            && x.EmployeeId == employeeId
                            && x.WorkDate == workDate, ct);

                    if (!existing)
                        continue;
                }

                var hasActual = actual is not null;
                var completed = actual is { Status: not null }
                    && string.Equals(actual.Status, "Completed", StringComparison.OrdinalIgnoreCase);
                var different = actual is not null && completed
                    && (actual.ActualStartDate?.Date != row.StartDate.Date
                        || actual.ActualEndDate?.Date != row.EndDate.Date);

                var status = cancelled
                    ? "Resolved"
                    : !hasActual
                        ? plannedEnd < today ? "Mismatch" : "None"
                        : completed
                            ? different ? "Mismatch" : "Matched"
                            : plannedEnd < today ? "Mismatch" : "None";

                await service.UpsertAsync(
                    row.EmployeeCode,
                    new ExecutionReconciliationUpsertRequest(
                        "TRIP",
                        "TRIP_REQUEST",
                        row.TripCode,
                        null,
                        employeeId,
                        workDate,
                        cancelled
                            ? "CANCELLED"
                            : $"Approved={plannedStart:yyyy-MM-dd}..{plannedEnd:yyyy-MM-dd}",
                        cancelled
                            ? "CANCELLED"
                            : actual is null
                                ? "NO_ACTUAL"
                                : $"Actual={actual.ActualStartDate:yyyy-MM-dd}..{actual.ActualEndDate:yyyy-MM-dd};Status={actual.Status}",
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
                            Actual = actual,
                            Different = different,
                            row.RequestStatus
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

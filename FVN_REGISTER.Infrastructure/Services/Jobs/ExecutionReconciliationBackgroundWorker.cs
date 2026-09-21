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

        if (policies.TryGetValue("ATTENDANCE", out var attendancePolicy))
            await ReconcileAttendanceAsync(db, reconciliation, from, to, attendancePolicy.ReconciliationMode, ct);
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

    private async Task ReconcileAttendanceAsync(
        FVNWEBAPPContext db,
        IExecutionReconciliationService service,
        DateOnly from,
        DateOnly to,
        byte reconciliationMode,
        CancellationToken ct)
    {
        var unresolved = reconciliationMode == 2
            ? await GetUnresolvedSourceIdsAsync(db, "ATTENDANCE", ct)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // OT actual-only reconciliation is owned by the OT module. We still
        // discover its source rows from the official attendance calculation,
        // including unresolved OT mismatches outside the normal worker window.
        var unresolvedOt = reconciliationMode == 2
            ? await GetUnresolvedSourceIdsAsync(db, "OT", ct)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var unresolvedOtKeys = unresolvedOt
            .Select(x => x.Split(':', 2))
            .Where(x => x.Length == 2 && DateOnly.TryParseExact(x[1], "yyyyMMdd", out _))
            .Select(x => new
            {
                EmployeeCode = x[0],
                WorkDate = DateOnly.ParseExact(x[1], "yyyyMMdd")
            })
            .ToList();

        var unresolvedOtEmployeeCodes = unresolvedOtKeys
            .Select(x => x.EmployeeCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unresolvedOtMinDate = unresolvedOtKeys.Count == 0
            ? from
            : unresolvedOtKeys.Min(x => x.WorkDate);
        var unresolvedOtMaxDate = unresolvedOtKeys.Count == 0
            ? to
            : unresolvedOtKeys.Max(x => x.WorkDate);

        var rows = await db.Database.SqlQuery<AttendanceWorkerRow>($"""
            SELECT
                a.WorkDate,
                a.EmployeeCode,
                a.CheckInTime,
                a.CheckOutTime,
                a.WorkMinutesDay,
                a.WorkMinutesNight,
                a.OTMinutesDay,
                a.OTMinutesNight,
                a.OTMinutesDayTC,
                a.OTMinutesNightTC,
                a.OTRecognizedMinutesDay,
                a.OTRecognizedMinutesNight,
                a.RequiredMinutes,
                a.LateMinutesDay,
                a.LateMinutesNight,
                a.EarlyLeaveMinutesDay,
                a.EarlyLeaveMinutesNight,
                a.LeaveTotal,
                a.HrmHoliday,
                a.HrmEmployeeHoliday,
                a.HrmBCLyDoNghi,
                a.HrmBCGhiChu
            FROM dbo.F03HrmAttendanceCalculated AS a
            WHERE (a.WorkDate >= {from}
              AND a.WorkDate <= {to})
               OR CONCAT(a.EmployeeCode, N':', CONVERT(char(8), a.WorkDate, 112)) IN (SELECT value FROM STRING_SPLIT({string.Join(",", unresolved)}, N','))
               OR (a.EmployeeCode IN (SELECT value FROM STRING_SPLIT({string.Join(",", unresolvedOtEmployeeCodes)}, N','))
                   AND a.WorkDate >= {unresolvedOtMinDate}
                   AND a.WorkDate <= {unresolvedOtMaxDate})
            ORDER BY a.WorkDate, a.EmployeeCode
            """).ToListAsync(ct);

        var approvedOt = await db.OvertimeEmployees.AsNoTracking()
            .Where(x => x.OTRequest.RequestStatus == ApprovalStatus.Approved
                && x.OTRequest.IsActive != false
                && (
                    (x.OTRequest.OTDate >= from.ToDateTime(TimeOnly.MinValue)
                        && x.OTRequest.OTDate <= to.ToDateTime(TimeOnly.MaxValue))
                    || (unresolvedOtEmployeeCodes.Contains(x.EmployeeCode)
                        && x.OTRequest.OTDate >= unresolvedOtMinDate.ToDateTime(TimeOnly.MinValue)
                        && x.OTRequest.OTDate <= unresolvedOtMaxDate.ToDateTime(TimeOnly.MaxValue))))
            .Select(x => new { x.EmployeeCode, x.OTRequest.OTDate })
            .ToListAsync(ct);

        var approvedOtDates = approvedOt
            .Select(x => $"{x.EmployeeCode}:{DateOnly.FromDateTime(x.OTDate):yyyyMMdd}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var today = DateOnly.FromDateTime(DateTime.Today);

        foreach (var row in rows)
        {
            var workDate = row.WorkDate;
            var sourceId = $"{row.EmployeeCode}:{workDate:yyyyMMdd}";
            if (workDate < from || workDate > to)
            {
                if (!unresolved.Contains(sourceId))
                    continue;
            }

            try
            {
                var employeeId = await ResolveEmployeeIdAsync(db, row.EmployeeCode, ct);
                var actualOtMinutes = Math.Max(0, row.OTMinutesDay)
                    + Math.Max(0, row.OTMinutesNight)
                    + Math.Max(0, row.OTMinutesDayTC)
                    + Math.Max(0, row.OTMinutesNightTC);
                var recognizedOtMinutes = Math.Max(0, row.OTRecognizedMinutesDay)
                    + Math.Max(0, row.OTRecognizedMinutesNight);
                var hasActualOt = actualOtMinutes > 0 || recognizedOtMinutes > 0;

                var hasActual = row.CheckInTime.HasValue
                    || row.CheckOutTime.HasValue
                    || row.WorkMinutesDay + row.WorkMinutesNight > 0;
                var isLeave = (row.LeaveTotal ?? 0m) > 0m;
                var isHoliday = row.HrmHoliday == true || row.HrmEmployeeHoliday == true;
                var hasApprovedOt = approvedOtDates.Contains(sourceId);
                var hasShiftPlan = row.RequiredMinutes > 0;
                var hasPlannedWork = hasShiftPlan || hasApprovedOt;
                var missingPunch = hasShiftPlan && (!row.CheckInTime.HasValue || !row.CheckOutTime.HasValue);
                var exception = row.LateMinutesDay + row.LateMinutesNight > 0
                    || row.EarlyLeaveMinutesDay + row.EarlyLeaveMinutesNight > 0;
                // Regular attendance mismatch and OT registration mismatch are
                // separate business signals. A day with actual OT but no OT
                // request must not be swallowed by the fact that the employee
                // also has a normal shift plan.
                var actualWithoutPlan = hasActual && !hasPlannedWork && !isLeave && !hasActualOt;
                var otWithoutRequest = hasActualOt && !hasApprovedOt;
                var pastExpectedWithoutActual = !hasActual && hasPlannedWork && workDate < today && !isLeave;

                var mismatch = actualWithoutPlan || missingPunch || exception || pastExpectedWithoutActual;
                var status = mismatch ? "Mismatch" : hasPlannedWork || hasActual ? "Matched" : "None";

                var plannedState = hasShiftPlan && hasApprovedOt
                    ? "SHIFT+OT"
                    : hasShiftPlan
                        ? "SHIFT"
                        : hasApprovedOt ? "OT" : "NONE";
                var actualState = hasActual ? "PRESENT" : "ABSENT";

                if (otWithoutRequest)
                {
                    // OT is projected under ModuleCode=OT so the existing OT
                    // calendar provider can render it. SourceType distinguishes
                    // this synthetic actual-only reconciliation from a real OT
                    // request and keeps it idempotent.
                    await service.UpsertAsync(
                        row.EmployeeCode,
                        new ExecutionReconciliationUpsertRequest(
                            "OT",
                            "OT_ACTUAL_ONLY",
                            sourceId,
                            row.EmployeeCode,
                            employeeId,
                            workDate,
                            "NONE",
                            $"ActualOTMinutes={actualOtMinutes};RecognizedOTMinutes={recognizedOtMinutes}",
                            "Mismatch",
                            true,
                            true,
                            JsonSerializer.Serialize(new
                            {
                                row.EmployeeCode,
                                WorkDate = workDate,
                                ActualOTMinutes = actualOtMinutes,
                                RecognizedOTMinutes = recognizedOtMinutes,
                                row.OTMinutesDay,
                                row.OTMinutesNight,
                                row.OTMinutesDayTC,
                                row.OTMinutesNightTC,
                                row.OTRecognizedMinutesDay,
                                row.OTRecognizedMinutesNight,
                                HasApprovedOt = false
                            })),
                        ct,
                        actorUserId: 0);
                }
                else if (hasActualOt && hasApprovedOt)
                {
                    // If a registration was subsequently created, retire the
                    // previous "actual without request" reconciliation so the
                    // red ? does not remain after the plan exists.
                    var actualOnly = await db.ExecutionReconciliations.FirstOrDefaultAsync(x =>
                        x.IsActive != false
                        && x.ModuleCode == "OT"
                        && x.SourceType == "OT_ACTUAL_ONLY"
                        && x.SourceId == sourceId
                        && x.EmployeeId == employeeId
                        && x.WorkDate == workDate
                        && x.ReconciliationStatus != "Resolved", ct);

                    if (actualOnly is not null)
                    {
                        await service.UpsertAsync(
                            row.EmployeeCode,
                            new ExecutionReconciliationUpsertRequest(
                                "OT",
                                "OT_ACTUAL_ONLY",
                                sourceId,
                                row.EmployeeCode,
                                employeeId,
                                workDate,
                                "SHIFT/OT",
                                $"ActualOTMinutes={actualOtMinutes};RecognizedOTMinutes={recognizedOtMinutes}",
                                "Resolved",
                                false,
                                false,
                                JsonSerializer.Serialize(new
                                {
                                    row.EmployeeCode,
                                    WorkDate = workDate,
                                    ActualOTMinutes = actualOtMinutes,
                                    RecognizedOTMinutes = recognizedOtMinutes,
                                    HasApprovedOt = true,
                                    AutoResolvedReason = "OT_REQUEST_EXISTS"
                                })),
                            ct,
                            actorUserId: 0);
                    }
                }
                else if (!hasActualOt)
                {
                    // Recalculation/correction may remove previously detected
                    // OT. Retire the synthetic mismatch so Calendar cannot keep
                    // showing a stale red question mark.
                    var staleActualOnly = await db.ExecutionReconciliations.FirstOrDefaultAsync(x =>
                        x.IsActive != false
                        && x.ModuleCode == "OT"
                        && x.SourceType == "OT_ACTUAL_ONLY"
                        && x.SourceId == sourceId
                        && x.EmployeeId == employeeId
                        && x.WorkDate == workDate
                        && x.ReconciliationStatus != "Resolved", ct);

                    if (staleActualOnly is not null)
                    {
                        await service.UpsertAsync(
                            row.EmployeeCode,
                            new ExecutionReconciliationUpsertRequest(
                                "OT",
                                "OT_ACTUAL_ONLY",
                                sourceId,
                                row.EmployeeCode,
                                employeeId,
                                workDate,
                                "NONE",
                                "NO_ACTUAL_OT",
                                "Resolved",
                                false,
                                false,
                                JsonSerializer.Serialize(new
                                {
                                    row.EmployeeCode,
                                    WorkDate = workDate,
                                    HasApprovedOt = hasApprovedOt,
                                    AutoResolvedReason = "ACTUAL_OT_NO_LONGER_PRESENT"
                                })),
                            ct,
                            actorUserId: 0);
                    }
                }

                await service.UpsertAsync(
                    row.EmployeeCode,
                    new ExecutionReconciliationUpsertRequest(
                        "ATTENDANCE",
                        "ATTENDANCE_DAY",
                        sourceId,
                        null,
                        employeeId,
                        workDate,
                        plannedState,
                        actualState,
                        status,
                        mismatch,
                        mismatch,
                        JsonSerializer.Serialize(new
                        {
                            row.EmployeeCode,
                            WorkDate = workDate,
                            Planned = new { plannedState, hasShiftPlan, hasApprovedOt },
                            Actual = new
                            {
                                hasActual,
                                row.CheckInTime,
                                row.CheckOutTime,
                                row.WorkMinutesDay,
                                row.WorkMinutesNight,
                                row.OTMinutesDay,
                                row.OTMinutesNight,
                                row.OTMinutesDayTC,
                                row.OTMinutesNightTC,
                                row.OTRecognizedMinutesDay,
                                row.OTRecognizedMinutesNight,
                                row.LateMinutesDay,
                                row.LateMinutesNight,
                                row.EarlyLeaveMinutesDay,
                                row.EarlyLeaveMinutesNight
                            },
                            isLeave,
                            isHoliday,
                            actualWithoutPlan,
                            missingPunch,
                            exception,
                            pastExpectedWithoutActual,
                            row.HrmBCLyDoNghi,
                            row.HrmBCGhiChu
                        })),
                    ct,
                    actorUserId: 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Execution reconciliation skipped Attendance {EmployeeCode}/{Date}.", row.EmployeeCode, workDate);
            }
            finally
            {
                db.ChangeTracker.Clear();
            }
        }
    }

    private sealed class AttendanceWorkerRow
    {
        public DateOnly WorkDate { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int WorkMinutesDay { get; set; }
        public int WorkMinutesNight { get; set; }
        public int OTMinutesDay { get; set; }
        public int OTMinutesNight { get; set; }
        public int OTMinutesDayTC { get; set; }
        public int OTMinutesNightTC { get; set; }
        public int OTRecognizedMinutesDay { get; set; }
        public int OTRecognizedMinutesNight { get; set; }
        public int RequiredMinutes { get; set; }
        public int LateMinutesDay { get; set; }
        public int LateMinutesNight { get; set; }
        public int EarlyLeaveMinutesDay { get; set; }
        public int EarlyLeaveMinutesNight { get; set; }
        public decimal? LeaveTotal { get; set; }
        public bool? HrmHoliday { get; set; }
        public bool? HrmEmployeeHoliday { get; set; }
        public string? HrmBCLyDoNghi { get; set; }
        public string? HrmBCGhiChu { get; set; }
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

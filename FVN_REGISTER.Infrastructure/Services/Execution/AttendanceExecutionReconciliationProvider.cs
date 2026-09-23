using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Contract.Dtos.Execution;
using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Execution;

public sealed class AttendanceExecutionReconciliationProvider : ExecutionReconciliationModuleProviderBase, IExecutionReconciliationModuleProvider, IPlannedProvider, IActualProvider, IReconciliationMapper
{
    public AttendanceExecutionReconciliationProvider(
        FVNWEBAPPContext db,
        IExecutionReconciliationService service,
        ILogger<AttendanceExecutionReconciliationProvider> logger)
        : base(db, service, logger) { }

    public string ModuleCode => "ATTENDANCE";

    public async Task ReconcileAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct)
    {
        var unresolved = reconciliationMode == 2
            ? await GetUnresolvedSourceIdsAsync(_db, "ATTENDANCE", ct)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // OT actual-only reconciliation is owned by the OT module. We still
        // discover its source rows from the official attendance calculation,
        // including unresolved OT mismatches outside the normal worker window.
        var unresolvedOt = reconciliationMode == 2
            ? await GetUnresolvedSourceIdsAsync(_db, "OT", ct)
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

        var rows = await _db.Database.SqlQuery<AttendanceWorkerRow>($"""
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

        var approvedOt = await _db.OvertimeEmployees.AsNoTracking()
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
                var employeeId = await ResolveEmployeeIdAsync(_db, row.EmployeeCode, ct);
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
                    await _service.UpsertAsync(
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
                    var actualOnly = await _db.ExecutionReconciliations.FirstOrDefaultAsync(x =>
                        x.IsActive != false
                        && x.ModuleCode == "OT"
                        && x.SourceType == "OT_ACTUAL_ONLY"
                        && x.SourceId == sourceId
                        && x.EmployeeId == employeeId
                        && x.WorkDate == workDate
                        && x.ReconciliationStatus != "Resolved", ct);

                    if (actualOnly is not null)
                    {
                        await _service.UpsertAsync(
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
                                }),
                                false),
                            ct,
                            actorUserId: 0);
                    }
                }
                else if (!hasActualOt)
                {
                    // Recalculation/correction may remove previously detected
                    // OT. Retire the synthetic mismatch so Calendar cannot keep
                    // showing a stale red question mark.
                    var staleActualOnly = await _db.ExecutionReconciliations.FirstOrDefaultAsync(x =>
                        x.IsActive != false
                        && x.ModuleCode == "OT"
                        && x.SourceType == "OT_ACTUAL_ONLY"
                        && x.SourceId == sourceId
                        && x.EmployeeId == employeeId
                        && x.WorkDate == workDate
                        && x.ReconciliationStatus != "Resolved", ct);

                    if (staleActualOnly is not null)
                    {
                        await _service.UpsertAsync(
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
                                }),
                                false),
                            ct,
                            actorUserId: 0);
                    }
                }

                await _service.UpsertAsync(
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
                _db.ChangeTracker.Clear();
            }
        }
    }

    public Task ReconcilePlannedAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct)
        => ReconcileAsync(from, to, reconciliationMode, ct);

    public Task ReconcileActualAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct)
        => ReconcileAsync(from, to, reconciliationMode, ct);

    public Task MapAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct)
        => ReconcileAsync(from, to, reconciliationMode, ct);
}

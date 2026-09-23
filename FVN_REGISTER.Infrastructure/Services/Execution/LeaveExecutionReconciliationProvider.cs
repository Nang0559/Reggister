using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Contract.Dtos.Execution;
using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Execution;

public sealed class LeaveExecutionReconciliationProvider : ExecutionReconciliationModuleProviderBase, IExecutionReconciliationModuleProvider, IPlannedProvider, IActualProvider, IReconciliationMapper
{
    public LeaveExecutionReconciliationProvider(
        FVNWEBAPPContext db,
        IExecutionReconciliationService service,
        ILogger<LeaveExecutionReconciliationProvider> logger)
        : base(db, service, logger) { }

    public string ModuleCode => "LEAVE";

    public async Task ReconcileAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct)
    {
        var unresolved = reconciliationMode == 2
            ? await GetUnresolvedSourceIdsAsync(_db, "LEAVE", ct)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var unresolvedLeaveIds = unresolved
            .Select(x => x.Split(':', 2)[0])
            .Select(x => int.TryParse(x, out var id) ? (int?)id : null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        var rows = await _db.VF03LeaveRequests.AsNoTracking()
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
        var details = await _db.Set<F03LeaveDayDetail>().AsNoTracking()
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
                    var employeeId = await ResolveEmployeeIdAsync(_db, row.EmployeeCode, ct);
                    var cancelled = row.RequestStatus == ApprovalStatus.Cancelled;

                    if (cancelled)
                    {
                        var existing = await _db.ExecutionReconciliations.AsNoTracking()
                            .AnyAsync(x => x.IsActive != false
                                && x.ModuleCode == "LEAVE"
                                && x.SourceType == "LEAVE_DAY"
                                && x.SourceId == $"{row.Id}:{date:yyyyMMdd}"
                                && x.EmployeeId == employeeId
                                && x.WorkDate == date, ct);

                        if (!existing)
                            continue;
                    }

                    var attendance = await _db.VF03EmployeeAttendances.AsNoTracking()
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

                    await _service.UpsertAsync(
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
                    _db.ChangeTracker.Clear();
                }
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

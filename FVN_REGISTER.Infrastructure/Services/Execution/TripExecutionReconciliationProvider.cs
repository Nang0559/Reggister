using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Contract.Dtos.Execution;
using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Execution;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Execution;

public sealed class TripExecutionReconciliationProvider : ExecutionReconciliationModuleProviderBase, IExecutionReconciliationModuleProvider, IPlannedProvider, IActualProvider, IReconciliationMapper
{
    public TripExecutionReconciliationProvider(
        FVNWEBAPPContext db,
        IExecutionReconciliationService service,
        ILogger<TripExecutionReconciliationProvider> logger)
        : base(db, service, logger) { }

    public string ModuleCode => "TRIP";

    public async Task ReconcileAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct)
    {
        var unresolved = reconciliationMode == 2
            ? await GetUnresolvedSourceIdsAsync(_db, "TRIP", ct)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var requests = await _db.TripRequests.AsNoTracking()
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
                var employeeId = await ResolveEmployeeIdAsync(_db, row.EmployeeCode, ct);
                var actualRows = await _db.Database.SqlQuery<TripActualWorkerRow>(
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
                    var existing = await _db.ExecutionReconciliations.AsNoTracking()
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

                await _service.UpsertAsync(
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

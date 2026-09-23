using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Contract.Dtos.Execution;
using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Execution;

public sealed class OtExecutionReconciliationProvider : ExecutionReconciliationModuleProviderBase, IExecutionReconciliationModuleProvider, IPlannedProvider, IActualProvider, IReconciliationMapper
{
    public OtExecutionReconciliationProvider(
        FVNWEBAPPContext db,
        IExecutionReconciliationService service,
        ILogger<OtExecutionReconciliationProvider> logger)
        : base(db, service, logger) { }

    public string ModuleCode => "OT";

    public async Task ReconcileAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct)
    {
        var unresolved = reconciliationMode == 2
            ? await GetUnresolvedSourceIdsAsync(_db, "OT", ct)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rows = await _db.OvertimeEmployees.AsNoTracking()
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
                    var cancelledEmployeeId = await ResolveEmployeeIdAsync(_db, row.EmployeeCode, ct);
                    var existing = await _db.ExecutionReconciliations.AsNoTracking()
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

                await _service.UpsertAsync(
                    row.EmployeeCode,
                    new ExecutionReconciliationUpsertRequest(
                        "OT",
                        "OT_EMPLOYEE",
                        $"{row.OTCode}:{row.EmployeeCode}",
                        row.EmployeeCode,
                        await ResolveEmployeeIdAsync(_db, row.EmployeeCode, ct),
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

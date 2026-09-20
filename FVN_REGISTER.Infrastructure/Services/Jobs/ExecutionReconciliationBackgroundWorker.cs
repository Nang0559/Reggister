using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Execution;
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Entities.Views;
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
        // Initial delay prevents the worker from competing with application startup/migrations.
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

    private static async Task RunOnceAsync(IServiceProvider services, CancellationToken ct)
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

    private static async Task ReconcileOtAsync(
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
            var planned = row.OTHours;
            var actual = row.ActualHours;

            // The current day is not a mismatch merely because HRM has not
            // produced ActualHours yet. Reconcile it after the execution window closes.
            var workDate = DateOnly.FromDateTime(row.OTDate);
            var status = actual.HasValue
                ? Math.Abs(actual.Value - planned) <= 0.1m ? "Matched" : "Mismatch"
                : workDate < DateOnly.FromDateTime(DateTime.Today) ? "Mismatch" : "None";

            var actualState = actual.HasValue
                ? $"ActualHours={actual.Value:0.##}"
                : "NO_ACTUAL";

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
                    actualState,
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
            db.ChangeTracker.Clear();
        }
    }

    private static async Task ReconcileLeaveAsync(
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

            int employeeId;
            try
            {
                employeeId = await ResolveEmployeeIdAsync(db, row.EmployeeCode, ct);
            }
            catch (Exception ex)
            {
                // One stale/deleted employee must not block the remaining reconciliation rows.
                Console.WriteLine($"Execution reconciliation skipped Leave {row.Id}/{row.EmployeeCode}: {ex.Message}");
                db.ChangeTracker.Clear();
                continue;
            }

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                try
                {
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
                    Console.WriteLine($"Execution reconciliation skipped Leave {row.Id}/{date:yyyy-MM-dd}: {ex.Message}");
                    db.ChangeTracker.Clear();
                }
                finally
                {
                    db.ChangeTracker.Clear();
                }
            }
        }
    }

    private static async Task ReconcileTripAsync(
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
                        JsonSerializer.Serialize(new
                        {
                            row.Id,
                            row.TripCode,
                            row.EmployeeCode,
                            row.StartDate,
                            row.EndDate,
                            row.Destination,
                            row.Purpose,
                            ActualSource = "F03TripActual (missing)"
                        })),
                    ct);
                continue;
            }

            var actualRows = await db.Database.SqlQuery<TripActualWorkerRow>(
                $"SELECT TripRequestId, EmployeeCode, ActualStartDate, ActualEndDate, Status FROM dbo.F03TripActual WHERE TripRequestId = {row.Id}")
                .ToListAsync(ct);

            var actual = actualRows.FirstOrDefault();
            var endDate = DateOnly.FromDateTime(row.EndDate);
            var workDate = DateOnly.FromDateTime(row.StartDate);
            var status = actual is null
                ? endDate < DateOnly.FromDateTime(DateTime.Today) ? "Mismatch" : "None"
                : string.Equals(actual.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                    ? "Matched"
                    : endDate < DateOnly.FromDateTime(DateTime.Today) ? "Mismatch" : "None";

            await service.UpsertAsync(
                row.EmployeeCode,
                new ExecutionReconciliationUpsertRequest(
                    "TRIP",
                    "TRIP_REQUEST",
                    row.TripCode,
                    null,
                    employeeId,
                    DateOnly.FromDateTime(row.StartDate),
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
                        Actual = actual
                    })),
                ct,
                actorUserId: 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Execution reconciliation skipped Trip {row.Id}/{row.EmployeeCode}: {ex.Message}");
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

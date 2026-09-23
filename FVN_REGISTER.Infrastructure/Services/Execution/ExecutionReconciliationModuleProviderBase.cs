using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FVN_REGISTER.Application.Interfaces.Execution;

namespace FVN_REGISTER.Infrastructure.Services.Execution;

public abstract class ExecutionReconciliationModuleProviderBase
{
    protected readonly FVNWEBAPPContext _db;
    protected readonly IExecutionReconciliationService _service;
    protected readonly ILogger _logger;

    protected ExecutionReconciliationModuleProviderBase(
        FVNWEBAPPContext db,
        IExecutionReconciliationService service,
        ILogger logger)
    {
        _db = db;
        _service = service;
        _logger = logger;
    }

    protected async Task<HashSet<string>> GetUnresolvedSourceIdsAsync(
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

    protected static Task<int> ResolveEmployeeIdAsync(
        FVNWEBAPPContext db,
        string employeeCode,
        CancellationToken ct) =>
        db.Employees.AsNoTracking()
            .Where(x => x.IsActive != false && x.EmployeeCode == employeeCode)
            .Select(x => x.Id)
            .SingleAsync(ct);

    protected sealed class TripActualWorkerRow
    {
        public int TripRequestId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string? Status { get; set; }
    }

    protected sealed class AttendanceWorkerRow
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
}

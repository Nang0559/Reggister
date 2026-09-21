using System.Text;
using FVN_REGISTER.Application.Interfaces.Payroll;
using FVN_REGISTER.Contract.Dtos.Payroll;
using FVN_REGISTER.Core.Entities.Payroll;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Payroll;

public sealed class PayrollInputService : IPayrollInputService
{
    private readonly FVNWEBAPPContext _db;

    public PayrollInputService(FVNWEBAPPContext db) => _db = db;

    public async Task<PayrollPeriodDto> GetOrCreateCurrentPeriodAsync(int actorUserId, CancellationToken ct = default)
    {
        var rows = await _db.PayrollCalculationPeriods
            .FromSqlInterpolated($"EXEC dbo.usp_EnsurePayrollPeriod @AsOfDate={DateTime.Today}, @ActorUserId={actorUserId}")
            .AsNoTracking()
            .ToListAsync(ct);

        var row = rows.FirstOrDefault();

        return row is null
            ? throw new InvalidOperationException("Không thể tạo/xác định kỳ lương hiện tại.")
            : Map(row);
    }

    public async Task<IReadOnlyList<PayrollPeriodDto>> GetPeriodsAsync(CancellationToken ct = default)
        => await _db.PayrollCalculationPeriods.AsNoTracking()
            .Where(x => x.IsActive != false)
            .OrderByDescending(x => x.FromDate)
            .Select(x => new PayrollPeriodDto(
                x.Id, x.PeriodCode, x.FromDate, x.ToDate, x.Status,
                x.CalculatedAt, x.CalculatedBy, x.LockedAt, x.LockedBy,
                x.ExportedAt, x.ExportedBy))
            .ToListAsync(ct);

    public async Task<PayrollPrepareDto> PrepareAsync(int periodId, int actorUserId, CancellationToken ct = default)
    {
        var period = await GetPeriodAsync(periodId, ct);
        Ensure21To20(period);

        if (period.Status is "Locked" or "Exported")
            throw new InvalidOperationException("Kỳ lương đã khóa/xuất, không được chuẩn bị lại.");

        var results = await _db.Database.SqlQueryRaw<PayrollPrepareResult>(
            "EXEC dbo.usp_PreparePayrollPeriod @PeriodId={0}, @ActorUserId={1}",
            periodId, actorUserId)
            .ToListAsync(ct);

        var result = results.FirstOrDefault();

        return new PayrollPrepareDto(periodId, result?.InputRows ?? 0);
    }

    public async Task<PayrollPeriodDto> LockAsync(int periodId, int actorUserId, CancellationToken ct = default)
    {
        var period = await GetPeriodAsync(periodId, ct);
        Ensure21To20(period);

        if (period.Status != "Calculated")
            throw new InvalidOperationException("Chỉ được khóa kỳ lương ở trạng thái Calculated.");

        var calculatedAt = period.CalculatedAt
            ?? throw new InvalidOperationException("Kỳ lương chưa có thời điểm snapshot.");

        var inputCount = await _db.PayrollInputs.CountAsync(
            x => x.PayrollPeriodId == periodId && x.IsActive != false, ct);

        if (inputCount == 0)
            throw new InvalidOperationException("Không thể khóa kỳ lương rỗng.");

        await EnsurePayrollReadyAsync(period, ct);

        var stale = await _db.ExecutionReconciliations.AsNoTracking()
            .AnyAsync(x => x.IsActive != false
                && x.WorkDate >= period.FromDate
                && x.WorkDate <= period.ToDate
                && x.ModifiedAt.HasValue
                && x.ModifiedAt.Value > calculatedAt, ct);

        if (stale)
            throw new InvalidOperationException("Snapshot Payroll Input đã cũ. Hãy Prepare lại kỳ lương trước khi khóa.");

        var lockedRows = await _db.PayrollCalculationPeriods
            .FromSqlInterpolated($"EXEC dbo.usp_LockPayrollPeriod @PeriodId={periodId}, @ActorUserId={actorUserId}")
            .AsNoTracking()
            .ToListAsync(ct);

        var locked = lockedRows.FirstOrDefault()
            ?? throw new InvalidOperationException("Không thể khóa kỳ lương.");

        return Map(locked);
    }

    public async Task<PayrollExportDto> ExportAsync(int periodId, int actorUserId, CancellationToken ct = default)
    {
        var period = await GetPeriodAsync(periodId, ct);
        Ensure21To20(period);

        if (period.Status is not ("Locked" or "Exported"))
            throw new InvalidOperationException("Chỉ được xuất kỳ lương sau khi đã khóa.");

        await EnsurePayrollReadyAsync(period, ct);

        var rows = await GetInputsAsync(periodId, ct);
        if (rows.Count == 0)
            throw new InvalidOperationException("Kỳ lương không có Payroll Input để xuất.");

        var sb = new StringBuilder();
        sb.AppendLine("EmployeeCode,EmployeeName,WorkDate,WorkMinutes,LeaveTotal,OTMinutes,SnapshotAt");

        foreach (var row in rows)
        {
            sb.Append(Escape(row.EmployeeCode)).Append(',')
              .Append(Escape(row.EmployeeName)).Append(',')
              .Append(row.WorkDate.ToString("yyyy-MM-dd")).Append(',')
              .Append(row.WorkMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(',')
              .Append(row.LeaveTotal.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(',')
              .Append(row.OTMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(',')
              .Append(row.SnapshotAt.ToString("yyyy-MM-dd HH:mm:ss")).AppendLine();
        }

        var now = DateTime.Now;

        if (period.Status != "Exported")
        {
            period.Status = "Exported";
            period.ExportedAt = now;
            period.ExportedBy = actorUserId;
            period.ModifiedBy = actorUserId;
            period.ModifiedAt = now;
            period.LastModifiedSource = "PAYROLL_EXPORT";
            await _db.SaveChangesAsync(ct);
        }

        return new PayrollExportDto(
            period.Id,
            period.PeriodCode,
            period.ExportedAt ?? now,
            rows.Count,
            $"Payroll_{period.PeriodCode}.csv",
            "text/csv; charset=utf-8",
            new UTF8Encoding(true).GetBytes(sb.ToString()));
    }

    public async Task<PayrollPrintResultDto> GetPrintDataAsync(int periodId, CancellationToken ct = default)
    {
        var period = await GetPeriodAsync(periodId, ct);
        Ensure21To20(period);

        if (period.Status is not ("Calculated" or "Locked"))
            throw new InvalidOperationException(
                "Chỉ được in bảng công khi kỳ lương đã Calculated hoặc Locked.");

        await EnsurePayrollReadyAsync(period, ct);

        var calculatedAt = period.CalculatedAt
            ?? throw new InvalidOperationException("Kỳ lương chưa có snapshot chính thức.");

        var stale = await _db.ExecutionReconciliations.AsNoTracking()
            .AnyAsync(x => x.IsActive != false
                && x.WorkDate >= period.FromDate
                && x.WorkDate <= period.ToDate
                && x.ModifiedAt.HasValue
                && x.ModifiedAt.Value > calculatedAt, ct);

        if (stale)
            throw new InvalidOperationException(
                "Bảng công đã thay đổi sau lần snapshot. Hãy Prepare lại kỳ lương trước khi in.");

        var inputs = await GetInputsAsync(periodId, ct);
        if (inputs.Count == 0)
            throw new InvalidOperationException("Kỳ lương không có dữ liệu bảng công để in.");

        return new PayrollPrintResultDto(Map(period), inputs);
    }

    public async Task<IReadOnlyList<PayrollInputDto>> GetInputsAsync(int periodId, CancellationToken ct = default)
    {
        _ = await GetPeriodAsync(periodId, ct);

        // Sắp xếp trên cột của entity TRƯỚC, rồi mới chiếu sang DTO.
        return await (
            from input in _db.PayrollInputs.AsNoTracking()
            join employee in _db.Employees.AsNoTracking() on input.EmployeeId equals employee.Id
            where input.PayrollPeriodId == periodId && input.IsActive != false
            orderby employee.EmployeeCode, input.WorkDate
            select new PayrollInputDto(
                input.Id,
                input.PayrollPeriodId,
                input.EmployeeId,
                employee.EmployeeCode,
                employee.EmployeeName,
                input.WorkDate,
                input.WorkMinutes,
                input.LeaveTotal,
                input.OTMinutes,
                input.SnapshotAt))
            .ToListAsync(ct);
    }

    private async Task EnsurePayrollReadyAsync(
        F03PayrollCalculationPeriod period,
        CancellationToken ct)
    {
        var unresolved = await _db.ExecutionReconciliations.AsNoTracking()
            .AnyAsync(x => x.IsActive != false
                && x.WorkDate >= period.FromDate
                && x.WorkDate <= period.ToDate
                && x.ReconciliationStatus != "Resolved"
                && x.ReconciliationStatus != "Matched"
                && x.ReconciliationStatus != "None", ct);

        if (unresolved)
            throw new InvalidOperationException(
                "Kỳ lương còn Execution Reconciliation Mismatch/AwaitingConfirmation chưa được giải quyết.");

        var failedOrPendingCorrection = await _db.ExecutionCorrections.AsNoTracking()
            .AnyAsync(x => x.IsActive != false
                && x.WorkDate >= period.FromDate
                && x.WorkDate <= period.ToDate
                && x.Status != "Applied"
                && x.Status != "Cancelled", ct);

        if (failedOrPendingCorrection)
            throw new InvalidOperationException(
                "Kỳ lương còn Execution Correction Pending/Failed chưa được xử lý.");
    }

    private async Task<F03PayrollCalculationPeriod> GetPeriodAsync(
        int periodId,
        CancellationToken ct)
        => await _db.PayrollCalculationPeriods.FirstOrDefaultAsync(
            x => x.Id == periodId && x.IsActive != false, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");

    private static void Ensure21To20(F03PayrollCalculationPeriod period)
    {
        if (period.FromDate.Day != 21
            || period.ToDate != period.FromDate.AddMonths(1).AddDays(-1))
            throw new InvalidOperationException("Kỳ lương phải theo chu kỳ ngày 21 đến ngày 20.");
    }

    private static PayrollPeriodDto Map(F03PayrollCalculationPeriod x)
        => new(
            x.Id,
            x.PeriodCode,
            x.FromDate,
            x.ToDate,
            x.Status,
            x.CalculatedAt,
            x.CalculatedBy,
            x.LockedAt,
            x.LockedBy,
            x.ExportedAt,
            x.ExportedBy);

    private static string Escape(string? value)
        => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";

    private sealed class PayrollPrepareResult
    {
        public int PeriodId { get; set; }
        public int InputRows { get; set; }
    }
}

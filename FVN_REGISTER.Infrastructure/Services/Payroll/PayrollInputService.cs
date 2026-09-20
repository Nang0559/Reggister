using System.Text;
using FVN_REGISTER.Application.Interfaces.Payroll;
using FVN_REGISTER.Contract.Dtos.Payroll;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Payroll;

public sealed class PayrollInputService : IPayrollInputService
{
    private readonly FVNWEBAPPContext _db;

    public PayrollInputService(FVNWEBAPPContext db) => _db = db;

    public async Task<PayrollPeriodDto> GetOrCreateCurrentPeriodAsync(int actorUserId, CancellationToken ct = default)
    {
        var row = await _db.PayrollCalculationPeriods
            .FromSqlInterpolated($"EXEC dbo.usp_EnsurePayrollPeriod @AsOfDate={DateTime.Today}, @ActorUserId={actorUserId}")
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

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

        await EnsurePayrollReadyAsync(periodId, period, ct);

        var result = await _db.Database.SqlQueryRaw<PayrollPrepareResult>(
            "EXEC dbo.usp_PreparePayrollPeriod @PeriodId={0}, @ActorUserId={1}",
            periodId, actorUserId).FirstOrDefaultAsync(ct);

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

        await EnsurePayrollReadyAsync(periodId, period, ct);

        var stale = await _db.ExecutionReconciliations.AsNoTracking()
            .AnyAsync(x => x.IsActive != false
                && x.WorkDate >= period.FromDate
                && x.WorkDate <= period.ToDate
                && x.ModifiedAt.HasValue
                && x.ModifiedAt.Value > calculatedAt, ct);

        if (stale)
            throw new InvalidOperationException("Snapshot Payroll Input đã cũ. Hãy Prepare lại kỳ lương trước khi khóa.");

        period.Status = "Locked";
        period.LockedAt = DateTime.Now;
        period.LockedBy = actorUserId;
        period.ModifiedBy = actorUserId;
        period.ModifiedAt = DateTime.Now;
        period.LastModifiedSource = "PAYROLL_LOCK";
        await _db.SaveChangesAsync(ct);
        return Map(period);
    }

    public async Task<PayrollExportDto> ExportAsync(int periodId, int actorUserId, CancellationToken ct = default)
    {
        var period = await GetPeriodAsync(periodId, ct);
        Ensure21To20(period);

        if (period.Status is not ("Locked" or "Exported"))
            throw new InvalidOperationException("Chỉ được xuất kỳ lương sau khi đã khóa.");

        var rows = await GetInputsAsync(periodId, ct);
        if (rows.Count == 0)
            throw new InvalidOperationException("Kỳ lương không có Payroll Input để xuất.");

        var sb = new StringBuilder();
        sb.AppendLine("EmployeeCode,EmployeeName,WorkDate,WorkMinutes,LeaveTotal,OTMinutes,SnapshotAt");
        foreach (var r in rows)
        {
            sb.Append(Escape(r.EmployeeCode)).Append(',')
              .Append(Escape(r.EmployeeName)).Append(',')
              .Append(r.WorkDate.ToString("yyyy-MM-dd")).Append(',')
              .Append(r.WorkMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(',')
              .Append(r.LeaveTotal.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(',')
              .Append(r.OTMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(',')
              .Append(r.SnapshotAt.ToString("yyyy-MM-dd HH:mm:ss")).AppendLine();
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
            period.Id, period.PeriodCode, period.ExportedAt ?? now, rows.Count,
            $"Payroll_{period.PeriodCode}.csv", "text/csv; charset=utf-8",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(sb.ToString()));
    }

    public async Task<IReadOnlyList<PayrollInputDto>> GetInputsAsync(int periodId, CancellationToken ct = default)
    {
        _ = await GetPeriodAsync(periodId, ct);

        return await _db.PayrollInputs.AsNoTracking()
            .Where(x => x.PayrollPeriodId == periodId && x.IsActive != false)
            .Join(_db.Employees.AsNoTracking(),
                x => x.EmployeeId,
                e => e.Id,
                (x, e) => new PayrollInputDto(
                    x.Id, x.PayrollPeriodId, x.EmployeeId,
                    e.EmployeeCode, e.EmployeeName, x.WorkDate,
                    x.WorkMinutes, x.LeaveTotal, x.OTMinutes, x.SnapshotAt))
            .OrderBy(x => x.EmployeeCode)
            .ThenBy(x => x.WorkDate)
            .ToListAsync(ct);
    }

    private async Task<F03PayrollCalculationPeriod> GetPeriodAsync(int periodId, CancellationToken ct)
        => await _db.PayrollCalculationPeriods.FirstOrDefaultAsync(
            x => x.Id == periodId && x.IsActive != false, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");

    private async Task EnsurePayrollReadyAsync(int periodId, F03PayrollCalculationPeriod period, CancellationToken ct)
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
                "Không được in/xuất bảng công-OT: kỳ lương còn Execution Reconciliation chưa được giải quyết.");

        var pendingCorrection = await _db.ExecutionCorrections.AsNoTracking()
            .AnyAsync(x => x.IsActive != false
                && x.WorkDate >= period.FromDate
                && x.WorkDate <= period.ToDate
                && x.Status != "Applied"
                && x.Status != "Cancelled", ct);

        if (pendingCorrection)
            throw new InvalidOperationException(
                "Không được in/xuất bảng công-OT: kỳ lương còn Execution Correction Pending/Failed.");

        var failedCorrection = await _db.ExecutionCorrections.AsNoTracking()
            .AnyAsync(x => x.IsActive != false
                && x.WorkDate >= period.FromDate
                && x.WorkDate <= period.ToDate
                && x.Status == "Failed", ct);

        if (failedCorrection)
            throw new InvalidOperationException(
                "Không được in/xuất bảng công-OT: kỳ lương còn Correction Failed.");

        var unresolvedHr = await _db.ExecutionReconciliations.AsNoTracking()
            .AnyAsync(x => x.IsActive != false
                && x.WorkDate >= period.FromDate
                && x.WorkDate <= period.ToDate
                && x.ReconciliationStatus == "Resolved"
                && _db.ExecutionCorrections.Any(c => c.IsActive != false
                    && c.ReconciliationId == x.Id
                    && c.Status == "Failed"), ct);

        if (unresolvedHr)
            throw new InvalidOperationException(
                "Không được in/xuất bảng công-OT: có Resolution đã đóng nhưng correction thất bại.");
    }

    private static void Ensure21To20(F03PayrollCalculationPeriod period)
    {
        if (period.FromDate.Day != 21
            || period.ToDate != period.FromDate.AddMonths(1).AddDays(-1))
            throw new InvalidOperationException("Kỳ lương phải theo chu kỳ ngày 21 đến ngày 20.");
    }

    private static PayrollPeriodDto Map(F03PayrollCalculationPeriod x)
        => new(x.Id, x.PeriodCode, x.FromDate, x.ToDate, x.Status,
            x.CalculatedAt, x.CalculatedBy, x.LockedAt, x.LockedBy,
            x.ExportedAt, x.ExportedBy);

    private static string Escape(string value)
        => """ + (value ?? string.Empty).Replace(""", """") + """;

    private sealed class PayrollPrepareResult
    {
        public int PeriodId { get; set; }
        public int InputRows { get; set; }
    }
}

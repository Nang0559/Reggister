using FVN_REGISTER.Application.Interfaces.Payroll;
using Microsoft.EntityFrameworkCore;
namespace FVN_REGISTER.Infrastructure.Services.Payroll;
public sealed class PayrollInputService : IPayrollInputService
{
 private readonly FVNWEBAPPContext _db;
 public PayrollInputService(FVNWEBAPPContext db)=>_db=db;
 public async Task<int> PrepareAsync(int periodId,CancellationToken ct=default)
 {
  var period=await _db.PayrollCalculationPeriods.FirstOrDefaultAsync(x=>x.Id==periodId&&x.IsActive,ct)
   ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");
  if(period.Status is "Locked" or "Exported") throw new InvalidOperationException("Kỳ lương đã khóa/xuất.");
  var rows=await _db.Database.SqlQueryRaw<PayrollPrepareResult>(
   "EXEC dbo.usp_PreparePayrollPeriod @PeriodId={0}",periodId).ToListAsync(ct);
  return rows.FirstOrDefault()?.InputRows??0;
 }
 public async Task LockAsync(int periodId,CancellationToken ct=default)
 {
  var period=await _db.PayrollCalculationPeriods.FirstOrDefaultAsync(x=>x.Id==periodId&&x.IsActive,ct)
   ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");
  if(period.Status!="Calculated") throw new InvalidOperationException("Chỉ được khóa kỳ lương sau khi đã chuẩn bị Payroll Input.");
  period.Status="Locked"; period.LockedAt=DateTime.Now; await _db.SaveChangesAsync(ct);
 }
 private sealed class PayrollPrepareResult { public int PeriodId {get;set;} public int InputRows {get;set;} }
}

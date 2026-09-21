using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Repositories;
using Microsoft.Data.SqlClient;
using FVN_REGISTER.Infrastructure.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
namespace FVN_REGISTER.Infrastructure.Services.HrmSync;
public sealed class HrmAttendanceCalculationService : IHrmAttendanceCalculationService
{
 private readonly IUnitOfWork _uow;
 private readonly ILogger<HrmAttendanceCalculationService> _logger;
 private readonly FVNWEBAPPContext _db;
 public HrmAttendanceCalculationService(IUnitOfWork uow,ILogger<HrmAttendanceCalculationService> logger,FVNWEBAPPContext db){_uow=uow;_logger=logger;_db=db;}
 public async Task<ServiceResult<HrmAttendanceCalculationResultDto>> CalculateAsync(HrmAttendanceCalculationRequestDto request,string? triggeredBy=null,CancellationToken ct=default)
 {
  if(request.FromDate.Date>request.ToDate.Date) return ServiceResult<HrmAttendanceCalculationResultDto>.Fail("FromDate không được lớn hơn ToDate.");
  var dept=string.IsNullOrWhiteSpace(request.DeptCode)?null:request.DeptCode.Trim();
  var pDept=new SqlParameter("@DeptCode",SqlDbType.NVarChar,20){Value=(object?)dept??DBNull.Value};
  var pFrom=new SqlParameter("@FromDate",SqlDbType.Date){Value=request.FromDate.Date};
  var pTo=new SqlParameter("@ToDate",SqlDbType.Date){Value=request.ToDate.Date};
  var pBy=new SqlParameter("@TriggeredBy",SqlDbType.NVarChar,100){Value=(object?)triggeredBy??"SYSTEM"};
  _uow.SetCommandTimeout(1800);
  try
  {
   var rows=await _uow.SqlQueryRawAsync<HrmAttendanceCalculationResultDto>("EXEC dbo.usp_CalculateHrmAttendance @DeptCode,@FromDate,@ToDate,@TriggeredBy",ct,pDept,pFrom,pTo,pBy);
   var result=rows.FirstOrDefault();
   if(result != null)
   {
    var from=DateOnly.FromDateTime(request.FromDate.Date);
    var to=DateOnly.FromDateTime(request.ToDate.Date);
    var periods=await _db.PayrollCalculationPeriods
      .Where(x=>x.IsActive!=false && x.Status=="Calculated" && x.FromDate<=to && x.ToDate>=from)
      .ToListAsync(ct);
    foreach(var period in periods)
    {
      period.Status="Open";
      period.ModifiedAt=DateTime.Now;
      period.ModifiedBy=0;
      period.LastModifiedSource="HRM_ATTENDANCE_RECALC";
    }
    if(periods.Count>0) await _db.SaveChangesAsync(ct);
   }
   return result==null?ServiceResult<HrmAttendanceCalculationResultDto>.Fail("Không nhận được kết quả tính giờ."):ServiceResult<HrmAttendanceCalculationResultDto>.Ok(result);
  }
  catch(Exception ex){_logger.LogError(ex,"HRM-compatible attendance calculation failed. Dept={Dept}, From={From}, To={To}",dept,request.FromDate,request.ToDate);return ServiceResult<HrmAttendanceCalculationResultDto>.Fail("Tính giờ HRM-compatible thất bại.");}
  finally{_uow.SetCommandTimeout(30);}
 }
}

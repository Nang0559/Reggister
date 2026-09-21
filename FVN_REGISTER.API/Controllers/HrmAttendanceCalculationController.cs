using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;
namespace FVN_REGISTER.API.Controllers;
[Authorize]
[ApiController]
[Route("api/hrm-attendance-calculation")]
public sealed class HrmAttendanceCalculationController:BaseApiController
{
 private readonly IHrmAttendanceCalculationService _service;
 private readonly IAuthorizationService _authorization;
 private readonly IHrmAttendanceExcelExportService _excel;
 public HrmAttendanceCalculationController(IHrmAttendanceCalculationService service,IHrmAttendanceExcelExportService excel,FVN_REGISTER.Application.Interfaces.Users.ICurrentUserService currentUser,FVN_REGISTER.Application.Interfaces.Users.IUserLogService userLog,IAuthorizationService authorization,ILogger<HrmAttendanceCalculationController> logger,Microsoft.Extensions.Options.IOptionsMonitor<FVN_REGISTER.Application.Configuration.AuthDebugOptions> options):base(currentUser,userLog,logger,options){_service=service;_excel=excel;_authorization=authorization;}
 [HttpGet("export/attendance/{batchId:guid}")]
 public async Task<IActionResult> ExportAttendance(Guid batchId, CancellationToken ct)
 {
  if(UserInfo==null || !await _authorization.HasAsync(UserInfo,SecurityFunctionCodes.AttendanceExport,ct)) return Forbid();
  var result=await _excel.ExportAttendanceAsync(batchId,ct);
  if(!result.IsSuccess||result.Data is null) return BadRequest(ApiResponse<object>.Fail(result.Message??"Xuất chấm công thất bại."));
  return File(result.Data,"application/vnd.ms-excel",$"BangChamCong_{DateTime.Now:yyyyMMdd_HHmmss}.xls");
 }

 [HttpGet("export/ot/{batchId:guid}")]
 public async Task<IActionResult> ExportOt(Guid batchId, CancellationToken ct)
 {
  if(UserInfo==null || !await _authorization.HasAsync(UserInfo,SecurityFunctionCodes.AttendanceExport,ct)) return Forbid();
  var result=await _excel.ExportOtAsync(batchId,ct);
  if(!result.IsSuccess||result.Data is null) return BadRequest(ApiResponse<object>.Fail(result.Message??"Xuất OT thất bại."));
  return File(result.Data,"application/vnd.ms-excel",$"BangLamThem_{DateTime.Now:yyyyMMdd_HHmmss}.xls");
 }

 [HttpPost("calculate")]
 public async Task<IActionResult> Calculate([FromBody] HrmAttendanceCalculationRequestDto request,CancellationToken ct)
 {
  if(UserInfo==null)return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
  if(!await _authorization.HasAsync(UserInfo,SecurityFunctionCodes.AttendanceView,ct)) return Forbid();
  if(request.FromDate.Date>request.ToDate.Date)return BadRequest(ApiResponse<object>.Fail("Khoảng ngày không hợp lệ."));
  var result=await _service.CalculateAsync(request,UserInfo.EmployeeCode,ct);
  return result.IsSuccess?Ok(ApiResponse<HrmAttendanceCalculationResultDto>.Ok(result.Data!)):BadRequest(ApiResponse<object>.Fail(result.Message??"Tính giờ thất bại."));
 }
}

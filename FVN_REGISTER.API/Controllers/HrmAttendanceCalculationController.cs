using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FVN_REGISTER.API.Controllers;
[Authorize]
[ApiController]
[Route("api/hrm-attendance-calculation")]
public sealed class HrmAttendanceCalculationController:BaseApiController
{
 private readonly IHrmAttendanceCalculationService _service;
 private readonly IHrmAttendanceExcelExportService _excel;
 public HrmAttendanceCalculationController(IHrmAttendanceCalculationService service,IHrmAttendanceExcelExportService excel,FVN_REGISTER.Application.Interfaces.Users.ICurrentUserService currentUser,FVN_REGISTER.Application.Interfaces.Users.IUserLogService userLog,ILogger<HrmAttendanceCalculationController> logger,Microsoft.Extensions.Options.IOptionsMonitor<FVN_REGISTER.Application.Configuration.AuthDebugOptions> options):base(currentUser,userLog,logger,options){_service=service;_excel=excel;}
 [HttpGet("export/attendance/{batchId:guid}")]
 public async Task<IActionResult> ExportAttendance(Guid batchId, CancellationToken ct)
 {
  var result=await _excel.ExportAttendanceAsync(batchId,ct);
  if(!result.IsSuccess||result.Data is null) return BadRequest(ApiResponse<object>.Fail(result.Message??"Xuất chấm công thất bại."));
  return File(result.Data,"application/vnd.ms-excel",$"BangChamCong_{DateTime.Now:yyyyMMdd_HHmmss}.xls");
 }

 [HttpGet("export/ot/{batchId:guid}")]
 public async Task<IActionResult> ExportOt(Guid batchId, CancellationToken ct)
 {
  var result=await _excel.ExportOtAsync(batchId,ct);
  if(!result.IsSuccess||result.Data is null) return BadRequest(ApiResponse<object>.Fail(result.Message??"Xuất OT thất bại."));
  return File(result.Data,"application/vnd.ms-excel",$"BangLamThem_{DateTime.Now:yyyyMMdd_HHmmss}.xls");
 }

 [HttpPost("calculate")]
 public async Task<IActionResult> Calculate([FromBody] HrmAttendanceCalculationRequestDto request,CancellationToken ct)
 {
  if(UserInfo==null)return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
  if(request.FromDate.Date>request.ToDate.Date)return BadRequest(ApiResponse<object>.Fail("Khoảng ngày không hợp lệ."));
  var result=await _service.CalculateAsync(request,UserInfo.EmployeeCode,ct);
  return result.IsSuccess?Ok(ApiResponse<HrmAttendanceCalculationResultDto>.Ok(result.Data!)):BadRequest(ApiResponse<object>.Fail(result.Message??"Tính giờ thất bại."));
 }
}

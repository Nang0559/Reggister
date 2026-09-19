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
 public HrmAttendanceCalculationController(IHrmAttendanceCalculationService service,FVN_REGISTER.Application.Interfaces.Users.ICurrentUserService currentUser,FVN_REGISTER.Application.Interfaces.Users.IUserLogService userLog,ILogger<HrmAttendanceCalculationController> logger,Microsoft.Extensions.Options.IOptionsMonitor<FVN_REGISTER.Application.Configuration.AuthDebugOptions> options):base(currentUser,userLog,logger,options)=>_service=service;
 [HttpPost("calculate")]
 public async Task<IActionResult> Calculate([FromBody] HrmAttendanceCalculationRequestDto request,CancellationToken ct)
 {
  if(UserInfo==null)return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
  if(request.FromDate.Date>request.ToDate.Date)return BadRequest(ApiResponse<object>.Fail("Khoảng ngày không hợp lệ."));
  var result=await _service.CalculateAsync(request,UserInfo.EmployeeCode,ct);
  return result.IsSuccess?Ok(ApiResponse<HrmAttendanceCalculationResultDto>.Ok(result.Data!)):BadRequest(ApiResponse<object>.Fail(result.Message??"Tính giờ thất bại."));
 }
}

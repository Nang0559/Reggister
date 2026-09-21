
using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LeaveCalendarController : BaseApiController
{
    private readonly ILeaveQueryService _queryService;
    private readonly IAuthorizationService _authorization;

    public LeaveCalendarController(
        ILeaveQueryService queryService,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        IAuthorizationService authorization,
    
        ILogger<LeaveCalendarController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options)
    {
        _queryService = queryService;
        _authorization = authorization;
    }

    [HttpGet("data")]
    public async Task<IActionResult> GetData(
        [FromQuery] string? empCode,
        [FromQuery] string? deptCode,
        [FromQuery] string? cvCode,
        [FromQuery] int year,
        CancellationToken ct)
    {
        if (UserInfo == null)
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

        var effectiveEmpCode = string.IsNullOrWhiteSpace(empCode)
            ? UserInfo.EmployeeCode ?? string.Empty
            : empCode;
        var effectiveDeptCode = string.IsNullOrWhiteSpace(deptCode)
            ? UserInfo.DeptCode ?? string.Empty
            : deptCode;
        var effectiveCvCode = string.IsNullOrWhiteSpace(cvCode)
            ? UserInfo.PositionCode ?? string.Empty
            : cvCode;

        try
        {
            if (!await _authorization.CanAccessAsync(UserInfo, SecurityFunctionCodes.LeaveView, effectiveEmpCode, effectiveDeptCode, ct))
                return Forbid();
            var masterData = await _queryService.GetCombinedDataAsync(
             effectiveEmpCode,
             effectiveDeptCode,
             effectiveCvCode,
             year,
             ct);

            var data = new LeaveCalendarDataDto
            {
                MasterData = masterData,
                UserLevel = UserInfo.LevelApprove
            };

           
            await LogActionAsync("Xem lịch đăng ký nghỉ");
            return Ok(ApiResponse<LeaveCalendarDataDto>.Ok(data));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[CALENDAR] GetData failed for EmpCode: {EmpCode}",
                effectiveEmpCode);
            return BadRequest(ApiResponse<object>.Fail("Không thể tải dữ liệu lịch"));
        }
    }
}

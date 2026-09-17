using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LeaveCalendarController : BaseApiController
{
    private readonly ILeaveQueryService _queryService;
    private readonly ILeaveService _leaveService;

    public LeaveCalendarController(
        ILeaveQueryService queryService,
        ILeaveService leaveService,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        IMapper mapper,
        ILogger<LeaveCalendarController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, mapper, logger, options)
    {
        _queryService = queryService;
        _leaveService = leaveService;
    }

    [HttpGet("data")]
    public async Task<IActionResult> GetData(
        [FromQuery] string empCode,
        [FromQuery] string deptCode,
        [FromQuery] string cvCode,
        [FromQuery] int year,
        CancellationToken ct)
    {
        var effectiveEmpCode = string.IsNullOrEmpty(empCode)
            ? UserInfo?.EmployeeCode ?? ""
            : empCode;
        var effectiveDeptCode = string.IsNullOrEmpty(deptCode)
            ? UserInfo?.DeptCode ?? ""
            : deptCode;
        var effectiveCvCode = string.IsNullOrEmpty(cvCode)
            ? UserInfo?.CvCode ?? ""
            : cvCode;

        try
        {
            var data = await _queryService.GetCombinedDataAsync(
                effectiveEmpCode,
                effectiveDeptCode,
                effectiveCvCode,
                year,
                ct);

            data.UserLevel = UserInfo?.LevelApprove ?? 0;
            await LogActionAsync("Xem lịch đăng ký nghỉ");
            return Ok(ApiResponse<SystemMasterDataDto>.Ok(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[CALENDAR] GetData error for EmpCode: {EmpCode}", effectiveEmpCode);
            return BadRequest(ApiResponse<object>.Fail("Không thể tải dữ liệu lịch"));
        }
    }
}

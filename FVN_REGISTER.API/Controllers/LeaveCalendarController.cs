
using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Responses;
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

    public LeaveCalendarController(
        ILeaveQueryService queryService,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        IMapper mapper,
        ILogger<LeaveCalendarController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, mapper, logger, options)
    {
        _queryService = queryService;
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
            ? UserInfo.CvCode ?? string.Empty
            : cvCode;

        try
        {
            var data = await _queryService.GetCombinedDataAsync(
                effectiveEmpCode,
                effectiveDeptCode,
                effectiveCvCode,
                year,
                ct);

            data.UserLevel = UserInfo.LevelApprove;
            await LogActionAsync("Xem lịch đăng ký nghỉ");
            return Ok(ApiResponse<SystemMasterDataDto>.Ok(data));
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

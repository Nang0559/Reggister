using AutoMapper;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
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

    /// <summary>
    /// Lấy toàn bộ dữ liệu cho trang Đăng ký nghỉ:
    /// ngày nghỉ CT, lịch cá nhân, loại phép, phép tồn, approvers
    /// </summary>
    [HttpGet("data")]
    public async Task<IActionResult> GetData(
        [FromQuery] string empCode,
        [FromQuery] string deptCode,
        [FromQuery] string cvCode,
        [FromQuery] int year,
        CancellationToken ct)
    {
        // Nếu empCode trống, lấy từ token
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

            return Ok(ApiResponse<CombinedHolidaysViewModel>.Ok(data));
        }
        catch (Exception ex)
        {
           // Logger.LogError(ex, "[CALENDAR] GetData error");
            return BadRequest(ApiResponse<object>.Fail("Không thể tải dữ liệu lịch"));
        }
    }
}

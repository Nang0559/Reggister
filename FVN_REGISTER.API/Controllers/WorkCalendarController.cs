using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/calendar")]
public sealed class WorkCalendarController : BaseApiController
{
    private readonly ISharedWorkCalendarService _calendar;
    private readonly IAuthorizationService _authorization;

    public WorkCalendarController(
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<WorkCalendarController> logger,
        IOptionsMonitor<AuthDebugOptions> options,
        ISharedWorkCalendarService calendar,
        IAuthorizationService authorization)
        : base(currentUser, userLog, logger, options)
    {
        _calendar = calendar;
        _authorization = authorization;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var modules = await GetAuthorizedModulesAsync(UserInfo, ct);
        if (modules.Count == 0)
            return Forbid();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var first = from ?? new DateOnly(today.Year, today.Month, 1);
        var last = to ?? new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        if (last < first)
            return BadRequest(ApiResponse<object>.Fail("Khoảng ngày không hợp lệ."));

        if (last.DayNumber - first.DayNumber > 93)
            return BadRequest(ApiResponse<object>.Fail("Lịch chỉ cho phép tối đa 94 ngày mỗi lần tải."));

        var result = await _calendar.GetMonthAsync(UserInfo.EmployeeCode, userId, first, last, modules, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("me/alerts")]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var modules = await GetAuthorizedModulesAsync(UserInfo, ct);
        if (modules.Count == 0)
            return Forbid();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var first = from ?? today.AddDays(-30);
        var last = to ?? today.AddDays(30);

        if (last < first)
            return BadRequest(ApiResponse<object>.Fail("Khoảng ngày không hợp lệ."));

        var result = await _calendar.GetAlertsAsync(UserInfo.EmployeeCode, userId, first, last, modules, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("me/availability")]
    public async Task<IActionResult> GetAvailability(
        [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(UserInfo?.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var result = await _calendar.GetAvailabilityAsync(
            UserInfo.EmployeeCode,
            date.ToDateTime(TimeOnly.MinValue),
            ct);

        return Ok(ApiResponse<object>.Ok(result));
    }

    private async Task<HashSet<string>> GetAuthorizedModulesAsync(
        FVN_REGISTER.Contract.Dtos.Authentication.UserIdentityDto user,
        CancellationToken ct)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (await _authorization.HasAsync(user, SecurityFunctionCodes.OTView, ct))
            result.Add("OT");

        if (await _authorization.HasAsync(user, SecurityFunctionCodes.LeaveView, ct))
            result.Add("LEAVE");

        if (await _authorization.HasAsync(user, SecurityFunctionCodes.TripView, ct))
            result.Add("TRIP");

        return result;
    }
}


using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/calendar")]
[Authorize]
public sealed class WorkCalendarController : ControllerBase
{
    private readonly IWorkCalendarService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;

    public WorkCalendarController(
        IWorkCalendarService service,
        ICurrentUserService currentUser,
        IAuthorizationService authorization)
    {
        _service = service;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<ActionResult<WorkCalendarDto>> Get(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();

        var canAttendance = await _authorization.HasAsync(user, SecurityFunctionCodes.AttendanceView, ct);
        var canLeave = await _authorization.HasAsync(user, SecurityFunctionCodes.LeaveView, ct);
        var canOt = await _authorization.HasAsync(user, SecurityFunctionCodes.OTView, ct);
        var canTrip = await _authorization.HasAsync(user, SecurityFunctionCodes.TripView, ct);

        if (!canAttendance && !canLeave && !canOt && !canTrip)
            return Forbid();

        var fromDate = from == default ? DateTime.Today : from.Date;
        var toDate = to == default ? fromDate.AddDays(42) : to.Date;

        if (toDate < fromDate)
            return BadRequest("Khoảng ngày không hợp lệ.");

        if ((toDate - fromDate).TotalDays > 93)
            return BadRequest("Lịch chỉ cho phép tối đa 94 ngày mỗi lần tải.");

        return Ok(await _service.GetAsync(
            user.EmployeeCode ?? string.Empty,
            user.DeptCode,
            user.PositionCode,
            fromDate,
            toDate,
            ct));
    }

    [HttpGet("availability")]
    public async Task<ActionResult<CalendarAvailabilityDto>> Availability(
        [FromQuery] DateTime date,
        CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();

        var canAttendance = await _authorization.HasAsync(user, SecurityFunctionCodes.AttendanceView, ct);
        var canLeave = await _authorization.HasAsync(user, SecurityFunctionCodes.LeaveView, ct);
        var canOt = await _authorization.HasAsync(user, SecurityFunctionCodes.OTView, ct);
        var canTrip = await _authorization.HasAsync(user, SecurityFunctionCodes.TripView, ct);

        if (!canAttendance && !canLeave && !canOt && !canTrip)
            return Forbid();

        return Ok(await _service.GetAvailabilityAsync(
            user.EmployeeCode ?? string.Empty, date, ct));
    }
}

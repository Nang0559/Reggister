using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Leaves;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/leave-entitlements")]
[Authorize]
public sealed class LeaveEntitlementController : ControllerBase
{
    private readonly ILeaveEntitlementService _service;
    private readonly ICurrentUserService _currentUser;

    public LeaveEntitlementController(
        ILeaveEntitlementService service,
        ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("me")]
    public async Task<ActionResult<LeaveEntitlementDto>> GetMine(
        [FromQuery] int? year,
        CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null || string.IsNullOrWhiteSpace(user.EmployeeCode))
            return Unauthorized();

        var workYear = year ?? DateTime.Today.Year;
        return Ok(await _service.EnsureCalculatedAsync(user.EmployeeCode, workYear, ct));
    }
}

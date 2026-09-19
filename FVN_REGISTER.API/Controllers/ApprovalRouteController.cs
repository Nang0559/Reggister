using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/approval-route")]
public sealed class ApprovalRouteController : ControllerBase
{
    private readonly IApprovalRouteService _routeService;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;

    public ApprovalRouteController(
        IApprovalRouteService routeService,
        ICurrentUserService currentUser,
        IAuthorizationService authorization)
    {
        _routeService = routeService;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    [HttpGet("preview")]
    public async Task<ActionResult<ApprovalRoutePreviewDto>> Preview(
        [FromQuery] RequestModule requestType,
        CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(user.EmployeeCode))
            return BadRequest("Tài khoản chưa có EmployeeCode.");

        var functionCode = requestType switch
        {
            RequestModule.Leave => SecurityFunctionCodes.LeaveCreate,
            RequestModule.Overtime => SecurityFunctionCodes.OTCreate,
            RequestModule.Trip => SecurityFunctionCodes.TripCreate,
            RequestModule.Equipment => SecurityFunctionCodes.EquipmentCreate,
            _ => 0
        };

        if (!await _authorization.HasAsync(user, functionCode, ct))
            return Forbid();

        if (requestType is not RequestModule.Leave
            and not RequestModule.Overtime
            and not RequestModule.Trip
            and not RequestModule.Equipment)
            return BadRequest("Loại yêu cầu không hỗ trợ.");

        var result = await _routeService.GetPreviewAsync(
            requestType,
            user.EmployeeCode,
            user.DeptCode ?? string.Empty,
            user.PositionCode ?? string.Empty,
            ct);

        return Ok(result);
    }
}

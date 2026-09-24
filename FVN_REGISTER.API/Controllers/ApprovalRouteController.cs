using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

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
    public async Task<ActionResult<ApiResponse<ApprovalRoutePreviewDto>>> Preview(
        [FromQuery] RequestModule requestType,
        CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();

        if (user == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(user.EmployeeCode))
            return BadRequest(
                ApiResponse<ApprovalRoutePreviewDto>.Fail(
                    "Tài khoản chưa có EmployeeCode."));

        var functionCode = requestType switch
        {
            RequestModule.Leave => SecurityFunctionCodes.LeaveCreate,
            RequestModule.Overtime => SecurityFunctionCodes.OTCreate,
            RequestModule.Trip => SecurityFunctionCodes.TripCreate,
            RequestModule.Equipment => SecurityFunctionCodes.EquipmentCreate,
            _ => 0
        };

        if (functionCode == 0)
        {
            return BadRequest(
                ApiResponse<ApprovalRoutePreviewDto>.Fail(
                    "Loại yêu cầu không hỗ trợ."));
        }

        if (!await _authorization.HasAsync(user, functionCode, ct))
            return Forbid();

        // EmployeeCode is the only requester identity sent into route resolution.
        // PositionCode/DeptCode are always loaded from F03Employee by the service.
        var result = await _routeService.GetPreviewAsync(
            requestType,
            user.EmployeeCode,
            ct);

        var response = ApiResponse<ApprovalRoutePreviewDto>.FromResult(result);

        if (!result.IsSuccess)
            return UnprocessableEntity(response);

        return Ok(response);
    }
}

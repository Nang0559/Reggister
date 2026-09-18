using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Requests.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/security")]
public sealed class SecurityController : BaseApiController
{
    private readonly IAuthorizationService _authorization;

    public SecurityController(
        IAuthorizationService authorization,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<SecurityController> logger,
        IOptionsMonitor<Application.Configuration.AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options)
    {
        _authorization = authorization;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyPermissions(CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        return Ok(ApiResponse<PermissionSnapshotDto>.Ok(
            await _authorization.GetSnapshotAsync(UserInfo.UserId, ct)));
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        if (!await CanManageAsync(SecurityFunctionCodes.SecurityView, ct))
            return Forbid();

        return Ok(ApiResponse<List<SecurityRoleDto>>.Ok(
            await _authorization.GetRolesAsync(ct)));
    }

    [HttpGet("functions")]
    public async Task<IActionResult> GetFunctions(CancellationToken ct)
    {
        if (!await CanManageAsync(SecurityFunctionCodes.SecurityView, ct))
            return Forbid();

        return Ok(ApiResponse<List<SecurityFunctionDto>>.Ok(
            await _authorization.GetFunctionsAsync(ct)));
    }

    [HttpGet("users/{userId:int}/permissions")]
    public async Task<IActionResult> GetUserPermissions(int userId, CancellationToken ct)
    {
        if (!await CanManageAsync(SecurityFunctionCodes.UserManagementView, ct))
            return Forbid();

        return Ok(ApiResponse<PermissionSnapshotDto>.Ok(
            await _authorization.GetSnapshotAsync(userId, ct)));
    }

    [HttpPut("users/{userId:int}/roles")]
    public async Task<IActionResult> SetUserRoles(
        int userId,
        [FromBody] UpdateUserRolesRequest request,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (userId != request.UserId)
            return BadRequest(ApiResponse<object>.Fail("UserId không khớp."));
        if (!await CanManageAsync(SecurityFunctionCodes.UserManagementAssignPermission, ct))
            return Forbid();

        try
        {
            var snapshot = await _authorization.SetUserRolesAsync(
                userId, request.RoleCodes, UserInfo.UserId, ct);

            await LogActionAsync($"Cập nhật role cho UserId={userId}");
            return Ok(ApiResponse<PermissionSnapshotDto>.Ok(snapshot));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    private async Task<bool> CanManageAsync(int functionCode, CancellationToken ct)
    {
        return UserInfo != null &&
               await _authorization.HasAsync(UserInfo, functionCode, ct);
    }
}

using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using AppAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/security")]
public sealed class SecurityController : BaseApiController
{
    private readonly AppAuthorizationService _authorization;
    private readonly ITwoFactorService _twoFactor;

    public SecurityController(
        IAuthorizationService authorization,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<SecurityController> logger,
        IOptionsMonitor<FVN_REGISTER.Application.Configuration.AuthDebugOptions> options,
        ITwoFactorService twoFactor)
        : base(currentUser, userLog, logger, options)
    {
        _authorization = authorization;
        _twoFactor = twoFactor;
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


    [HttpGet("users/{userId:int}/managed-scopes")]
    public async Task<IActionResult> GetManagedScopes(int userId, CancellationToken ct)
    {
        // ManagedScope is an assignment surface. Use the same capability for
        // reading/editing this configuration so the UI cannot open an editor
        // that the current user is unable to save.
        if (!await CanManageAsync(SecurityFunctionCodes.UserManagementAssignPermission, ct))
            return Forbid();

        return Ok(ApiResponse<List<ManagedScopeDto>>.Ok(
            await _authorization.GetManagedScopesAsync(userId, ct)));
    }

    [HttpPut("users/{userId:int}/managed-scopes")]
    public async Task<IActionResult> SetManagedScopes(
        int userId,
        [FromBody] UpdateManagedScopesRequest request,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (userId != request.UserId)
            return BadRequest(ApiResponse<object>.Fail("UserId không khớp."));
        if (!await CanManageAsync(SecurityFunctionCodes.UserManagementAssignPermission, ct))
            return Forbid();

        try
        {
            var snapshot = await _authorization.ReplaceManagedScopesAsync(
                userId, request.Scopes, UserInfo.UserId, ct);
            await LogActionAsync($"Cập nhật ManagedScope cho UserId={userId}");
            return Ok(ApiResponse<PermissionSnapshotDto>.Ok(snapshot));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }


    [HttpGet("users/2fa")]
    public async Task<IActionResult> GetTwoFactorUsers(CancellationToken ct)
    {
        if (!await CanManageTwoFactorAsync(ct))
            return Forbid();

        var users = await _authorization.GetTwoFactorUsersAsync(ct);
        return Ok(ApiResponse<List<TwoFactorAdminUserDto>>.Ok(users));
    }

    [HttpPut("users/{userId:int}/2fa-required")]
    public async Task<IActionResult> SetTwoFactorRequired(
        int userId,
        [FromBody] TwoFactorRequirementRequest request,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (!await CanManageAsync(SecurityFunctionCodes.UserManagementManageTwoFactor, ct))
            return Forbid();
        if (userId == UserInfo.UserId)
            return BadRequest(ApiResponse<object>.Fail("Không cho phép tự thay đổi chính sách 2 lớp của SuperAdmin."));

        var result = await _twoFactor.SetRequiredAsync(userId, request.Required, UserInfo.UserId, ct);
        if (result.IsSuccess)
            await LogActionAsync($"{(request.Required ? "Bật" : "Tắt")} bắt buộc 2FA cho UserId={userId}");
        return HandleResult(result);
    }

    [HttpPost("users/{userId:int}/2fa/reset")]
    public async Task<IActionResult> ResetUserTwoFactor(int userId, CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (!await CanManageTwoFactorAsync(ct))
            return Forbid();

        var result = await _twoFactor.ResetAsync(userId, UserInfo.UserId, ct);
        if (result.IsSuccess)
            await LogActionAsync($"Reset 2FA cho UserId={userId}");
        return HandleResult(result);
    }

    [HttpGet("me/managed-employees")]
    public async Task<IActionResult> GetMyManagedEmployees(CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.CalendarView, ct))
            return Forbid();

        return Ok(ApiResponse<List<ManagedEmployeeDto>>.Ok(
            await _authorization.GetManagedEmployeesAsync(UserInfo.UserId, ct)));
    }

    [HttpGet("users/{userId:int}/managed-employees")]
    public async Task<IActionResult> GetManagedEmployees(int userId, CancellationToken ct)
    {
        if (!await CanManageAsync(SecurityFunctionCodes.SecurityView, ct))
            return Forbid();

        return Ok(ApiResponse<List<ManagedEmployeeDto>>.Ok(
            await _authorization.GetManagedEmployeesAsync(userId, ct)));
    }

    [HttpGet("users/{userId:int}/effective-permission")]
    public async Task<IActionResult> GetEffectivePermissionPreview(int userId, CancellationToken ct)
    {
        if (!await CanManageAsync(SecurityFunctionCodes.SecurityView, ct))
            return Forbid();

        try
        {
            return Ok(ApiResponse<EffectivePermissionPreviewDto>.Ok(
                await _authorization.GetEffectivePermissionPreviewAsync(userId, ct)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPut("roles/{roleCode:int}/functions")]
    public async Task<IActionResult> SetRoleFunctions(
        int roleCode,
        [FromBody] UpdateRoleFunctionsRequest request,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (roleCode != request.RoleCode)
            return BadRequest(ApiResponse<object>.Fail("RoleCode không khớp."));
        if (!await CanManageAsync(SecurityFunctionCodes.SecurityManageFunctions, ct))
            return Forbid();

        try
        {
            var role = await _authorization.SetRoleFunctionsAsync(
                roleCode, request.FunctionCodes, UserInfo.UserId, ct);

            await LogActionAsync($"Cập nhật function cho RoleCode={roleCode}");
            return Ok(ApiResponse<SecurityRoleDto>.Ok(role));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    private async Task<bool> CanManageTwoFactorAsync(CancellationToken ct)
    {
        return UserInfo != null
            && UserInfo.Permission == UserPermissionCodes.SuperAdmin
            && await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.UserManagementManageTwoFactor, ct);
    }

    private async Task<bool> CanManageAsync(int functionCode, CancellationToken ct)
    {
        return UserInfo != null &&
               await _authorization.HasAsync(UserInfo, functionCode, ct);
    }
}

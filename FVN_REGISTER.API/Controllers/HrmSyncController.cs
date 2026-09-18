using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/hrm-sync")]
public sealed class HrmSyncController : BaseApiController
{
    private readonly IHrmSyncService _sync;

    public HrmSyncController(
        IHrmSyncService sync,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<HrmSyncController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options)
    {
        _sync = sync;
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        if (!CanManageHrmSync())
            return Forbid();

        return Ok(Contract.Responses.ApiResponse<Contract.Dtos.HrmSync.HrmSyncRuntimeStatusDto>.Ok(_sync.GetRuntimeStatus()));
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken ct)
    {
        if (!CanManageHrmSync())
            return Forbid();

        var user = UserInfo;
        var result = await _sync.RunAllAsync(user?.EmployeeCode ?? User.Identity?.Name ?? "ADMIN", manual: true, ct);
        await LogActionAsync("HRM master sync: run all");
        return HandleResult(result);
    }

    [HttpPost("run/{entityType}")]
    public async Task<IActionResult> RunEntity(string entityType, CancellationToken ct)
    {
        if (!CanManageHrmSync())
            return Forbid();

        if (string.IsNullOrWhiteSpace(entityType))
            return BadRequest("EntityType không được để trống.");

        var user = UserInfo;
        var result = await _sync.RunEntityAsync(entityType, user?.EmployeeCode ?? User.Identity?.Name ?? "ADMIN", ct);
        await LogActionAsync($"HRM master sync: {entityType}");
        return HandleResult(result);
    }

    private bool CanManageHrmSync()
        => UserInfo?.PermissionCode is int code
           && code >= UserPermissionCodes.SuperAdmin
           && code <= UserPermissionCodes.Editor;
}

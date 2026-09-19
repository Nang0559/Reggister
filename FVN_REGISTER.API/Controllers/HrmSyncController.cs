using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.HrmSync;
using AppAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;
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
    private readonly AppAuthorizationService _authorization;

    public HrmSyncController(
        IHrmSyncService sync,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<HrmSyncController> logger,
        IOptionsMonitor<AuthDebugOptions> options,
        AppAuthorizationService authorization)
        : base(currentUser, userLog, logger, options)
    {
        _sync = sync;
        _authorization = authorization;
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.HrmSyncViewStatus, ct))
            return Forbid();

        return Ok(ApiResponse<HrmSyncRuntimeStatusDto>.Ok(_sync.GetRuntimeStatus()));
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.HrmSyncSync, ct))
            return Forbid();

        var user = UserInfo;
        var result = await _sync.RunAllAsync(
            user?.EmployeeCode ?? User.Identity?.Name ?? "ADMIN",
            manual: true,
            ct);

        await LogActionAsync("HRM master sync: run all");
        return HandleResult(result);
    }

    [HttpPost("run/{entityType}")]
    public async Task<IActionResult> RunEntity(string entityType, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.HrmSyncSync, ct))
            return Forbid();

        if (string.IsNullOrWhiteSpace(entityType))
            return BadRequest("EntityType không được để trống.");

        var user = UserInfo;
        var result = await _sync.RunEntityAsync(
            entityType,
            user?.EmployeeCode ?? User.Identity?.Name ?? "ADMIN",
            ct);

        await LogActionAsync($"HRM master sync: {entityType}");
        return HandleResult(result);
    }


    private async Task<bool> CanAsync(int functionCode, CancellationToken ct)
        => UserInfo != null && await _authorization.HasAsync(UserInfo, functionCode, ct);
}

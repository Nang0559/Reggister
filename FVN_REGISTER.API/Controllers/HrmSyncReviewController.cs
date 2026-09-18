using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Core.Constants;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/hrm-sync/review")]
public sealed class HrmSyncReviewController : BaseApiController
{
    private readonly IHrmSyncReviewQueryService _review;
    private readonly IAuthorizationService _authorization;

    public HrmSyncReviewController(
        IHrmSyncReviewQueryService review,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<HrmSyncReviewController> logger,
        IOptionsMonitor<AuthDebugOptions> options,
        IAuthorizationService authorization)
        : base(currentUser, userLog, logger, options)
    {
        _review = review;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<IActionResult> GetUnresolved([FromQuery] string? entityType, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.HrmSyncReview, ct)) return Forbid();
        return HandleResult(await _review.GetUnresolvedAsync(entityType, ct));
    }

    [HttpGet("count")]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.HrmSyncReview, ct)) return Forbid();
        return HandleResult(await _review.CountUnresolvedAsync(ct));
    }

    [HttpPost("{flagId:int}/resolve")]
    public async Task<IActionResult> Resolve(int flagId, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.HrmSyncReview, ct)) return Forbid();
        return HandleResult(await _review.ResolveAsync(
            flagId,
            UserInfo?.EmployeeCode ?? User.Identity?.Name ?? "ADMIN",
            ct));
    }

    [HttpPost("resolve-by-entity")]
    public async Task<IActionResult> ResolveByEntity(
        [FromQuery] string entityType,
        [FromQuery] string entityKey,
        CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.HrmSyncReview, ct)) return Forbid();
        if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityKey))
            return BadRequest("entityType và entityKey không được để trống.");

        return HandleResult(await _review.ResolveByEntityAsync(
            entityType,
            entityKey,
            UserInfo?.EmployeeCode ?? User.Identity?.Name ?? "ADMIN",
            ct));
    }

    private async Task<bool> CanAsync(int functionCode, CancellationToken ct)
        => UserInfo != null && await _authorization.HasAsync(UserInfo, functionCode, ct);
}

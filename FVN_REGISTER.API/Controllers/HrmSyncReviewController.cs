using FVN_REGISTER.Application.Interfaces.HrmSync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

/// <summary>Admin review queue for HRM sync conflicts. Resolving a flag does not mutate source data.</summary>
[Authorize]
[ApiController]
[Route("api/hrm-sync/review")]
public sealed class HrmSyncReviewController : BaseApiController
{
    private readonly IHrmSyncReviewQueryService _review;

    public HrmSyncReviewController(
        IHrmSyncReviewQueryService review,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<HrmSyncReviewController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options)
    {
        _review = review;
    }

    [HttpGet]
    public async Task<IActionResult> GetUnresolved([FromQuery] string? entityType, CancellationToken ct)
        => HandleResult(await _review.GetUnresolvedAsync(entityType, ct));

    [HttpGet("count")]
    public async Task<IActionResult> Count(CancellationToken ct)
        => HandleResult(await _review.CountUnresolvedAsync(ct));

    [HttpPost("{flagId:int}/resolve")]
    public async Task<IActionResult> Resolve(int flagId, CancellationToken ct)
        => HandleResult(await _review.ResolveAsync(flagId, UserInfo?.EmployeeCode ?? User.Identity?.Name ?? "ADMIN", ct));

    [HttpPost("resolve-by-entity")]
    public async Task<IActionResult> ResolveByEntity([FromQuery] string entityType, [FromQuery] string entityKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityKey))
            return BadRequest("entityType và entityKey không được để trống.");

        return HandleResult(await _review.ResolveByEntityAsync(
            entityType,
            entityKey,
            UserInfo?.EmployeeCode ?? User.Identity?.Name ?? "ADMIN",
            ct));
    }
}

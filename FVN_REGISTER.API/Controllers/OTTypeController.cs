using FVN_REGISTER.Application.Interfaces.OTTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class OTTypeController : BaseApiController
{
    private readonly IOTTypeManagementService _service;

    public OTTypeController(
        IOTTypeManagementService service,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<OTTypeController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options)
    {
        _service = service;
    }

    /// <summary>Danh mục OT Type active dùng cho form đăng ký OT.</summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
        => HandleResult(await _service.GetFilteredAsync(true, ct));
}

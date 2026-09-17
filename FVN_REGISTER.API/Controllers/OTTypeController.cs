using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.OTTypes;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
        => HandleResult(await _service.GetFilteredAsync(true, ct));
}

using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/security/access-change")]
public sealed class AccessChangeController : BaseApiController
{
    private readonly IAccessChangeService _service;
    private readonly FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService _authorization;

    public AccessChangeController(
        IAccessChangeService service,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<AccessChangeController> logger,
        Microsoft.Extensions.Options.IOptionsMonitor<FVN_REGISTER.Application.Configuration.AuthDebugOptions> options,
        FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService authorization)
        : base(currentUser, userLog, logger, options)
    {
        _service = service;
        _authorization = authorization;
    }

    [HttpGet("employees")]
    public async Task<IActionResult> Employees(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.SecurityAccessChangeCreate, ct)) return Forbid();
        return HandleResult(await _service.GetEmployeeOptionsAsync(ct));
    }

    [HttpGet("functions")]
    public async Task<IActionResult> Functions([FromQuery] RequestModule businessModule, [FromQuery] string oldEmployeeCode, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.SecurityAccessChangeCreate, ct)) return Forbid();
        return HandleResult(await _service.GetFunctionOptionsAsync(businessModule, oldEmployeeCode, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AccessChangeRequestCreateDto request, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.SecurityAccessChangeCreate, ct)) return Forbid();
        return HandleResult(await _service.CreateAndSubmitAsync(request, ct));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.SecurityAccessChangeView, ct)) return Forbid();
        return HandleResult(await _service.GetMineAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        return HandleResult(await _service.GetAsync(id, ct));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromQuery] int level, [FromBody] string? comment, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.SecurityAccessChangeApprove, ct)) return Forbid();
        return HandleResult(await _service.ApproveAsync(id, level, comment, ct));
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromQuery] int level, [FromBody] string comment, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.SecurityAccessChangeApprove, ct)) return Forbid();
        return HandleResult(await _service.RejectAsync(id, level, comment, ct));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> Pending(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.SecurityAccessChangeApprove, ct)) return Forbid();
        return HandleResult(await _service.GetPendingApprovalsAsync(ct));
    }

    [HttpGet("it/queue")]
    public async Task<IActionResult> ItQueue(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.SecurityAccessChangeExecute, ct)) return Forbid();
        return HandleResult(await _service.GetItQueueAsync(ct));
    }

    [HttpPost("{id:int}/it-execute")]
    public async Task<IActionResult> ItExecute(int id, [FromBody] AccessChangeItExecuteDto request, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.SecurityAccessChangeExecute, ct)) return Forbid();
        return HandleResult(await _service.ExecuteByItAsync(id, request, ct));
    }

    private async Task<bool> CanAsync(int functionCode, CancellationToken ct)
        => UserInfo != null && await _authorization.HasAsync(UserInfo, functionCode, ct);
}

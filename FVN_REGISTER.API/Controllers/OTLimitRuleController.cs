using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.OTLimitRules;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/ot-limit-rules")]
public sealed class OTLimitRuleController : BaseApiController
{
    private readonly IOTLimitRuleManagementService _service;
    private readonly IAuthorizationService _authorization;

    public OTLimitRuleController(
        IOTLimitRuleManagementService service,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<OTLimitRuleController> logger,
        IOptionsMonitor<AuthDebugOptions> options,
        IAuthorizationService authorization)
        : base(currentUser, userLog, logger, options)
    {
        _service = service;
        _authorization = authorization;
    }

    private async Task<bool> CanManageAsync(CancellationToken ct)
        => UserInfo != null && await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.OTLimitManage, ct);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        return HandleResult(await _service.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        return HandleResult(await _service.GetByIdAsync(id, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OTLimitRuleUpsertDto model, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (UserInfo == null) return Unauthorized();
        return HandleResult(await _service.CreateAsync(model, UserInfo.UserId, ct));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] OTLimitRuleUpsertDto model, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (UserInfo == null) return Unauthorized();
        model.Id = id;
        return HandleResult(await _service.UpdateAsync(model, UserInfo.UserId, ct));
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        if (UserInfo == null) return Unauthorized();
        return HandleResult(await _service.ToggleActiveAsync(id, UserInfo.UserId, ct));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        return HandleResult(await _service.DeleteAsync(id, ct));
    }
}

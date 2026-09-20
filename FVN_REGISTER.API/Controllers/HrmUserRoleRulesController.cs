using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Requests.HrmSync;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/hrm-role-rules")]
public sealed class HrmUserRoleRulesController : BaseApiController
{
    private readonly IHrmUserRoleRuleService _service;

    public HrmUserRoleRulesController(
        IHrmUserRoleRuleService service,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<HrmUserRoleRulesController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        if (!CanManage()) return Forbid();
        return HandleResult(await _service.GetAllAsync(ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HrmUserRoleRuleRequest request, CancellationToken ct)
    {
        if (!CanManage()) return Forbid();
        var user = UserInfo;
        if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));
        return HandleResult(await _service.CreateAsync(request, user.UserId, ct));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] HrmUserRoleRuleRequest request, CancellationToken ct)
    {
        if (!CanManage()) return Forbid();
        var user = UserInfo;
        if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));
        return HandleResult(await _service.UpdateAsync(id, request, user.UserId, ct));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!CanManage()) return Forbid();
        return HandleResult(await _service.DeleteAsync(id, UserInfo!.UserId, ct));
    }

    private bool CanManage()
        => UserInfo?.PermissionCode is int code
           && code >= UserPermissionCodes.SuperAdmin
           && code <= UserPermissionCodes.Editor;
}
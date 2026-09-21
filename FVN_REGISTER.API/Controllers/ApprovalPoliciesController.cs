using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/approval-policies")]
public sealed class ApprovalPoliciesController : BaseApiController
{
    private readonly IApprovalPolicyService _service;
    private readonly IAuthorizationService _authorization;

    public ApprovalPoliciesController(
        IApprovalPolicyService service,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        IAuthorizationService authorization,
        ILogger<ApprovalPoliciesController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options)
    {
        _service = service;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => await CanManageAsync(ct) ? HandleResult(await _service.GetAllAsync(ct)) : Forbid();

    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions(CancellationToken ct)
        => await CanManageAsync(ct) ? HandleResult(await _service.GetPositionsAsync(ct)) : Forbid();

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments(CancellationToken ct)
        => await CanManageAsync(ct) ? HandleResult(await _service.GetDepartmentsAsync(ct)) : Forbid();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApprovalPolicyRequest request, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        var user = UserInfo;
        if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));
        return HandleResult(await _service.CreateAsync(request, user.UserId, ct));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ApprovalPolicyRequest request, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        var user = UserInfo;
        if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));
        return HandleResult(await _service.UpdateAsync(id, request, user.UserId, ct));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        return HandleResult(await _service.DeleteAsync(id, UserInfo!.UserId, ct));
    }

    private Task<bool> CanManageAsync(CancellationToken ct)
        => UserInfo != null ? _authorization.HasAsync(UserInfo, SecurityFunctionCodes.ApprovalPolicyManage, ct) : Task.FromResult(false);
}

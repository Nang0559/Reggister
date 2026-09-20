using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Payroll;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/payroll")]
public sealed class PayrollController : BaseApiController
{
    private readonly IPayrollInputService _service;
    private readonly IAuthorizationService _authorization;

    public PayrollController(
        IPayrollInputService service,
        IAuthorizationService authorization,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<PayrollController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options)
    {
        _service = service;
        _authorization = authorization;
    }

    [HttpGet("periods")]
    public async Task<IActionResult> GetPeriods(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.PayrollView, ct)) return Forbid();
        return Ok(ApiResponse<object>.Ok(await _service.GetPeriodsAsync(ct)));
    }

    [HttpPost("periods/current/ensure")]
    public async Task<IActionResult> EnsureCurrent(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.PayrollPrepare, ct)) return Forbid();
        var user = UserInfo!;
        return Ok(ApiResponse<object>.Ok(await _service.GetOrCreateCurrentPeriodAsync(user.UserId, ct)));
    }

    [HttpPost("periods/{periodId:int}/prepare")]
    public async Task<IActionResult> Prepare(int periodId, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.PayrollPrepare, ct)) return Forbid();
        var user = UserInfo!;
        return Ok(ApiResponse<object>.Ok(await _service.PrepareAsync(periodId, user.UserId, ct)));
    }

    [HttpPost("periods/{periodId:int}/lock")]
    public async Task<IActionResult> Lock(int periodId, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.PayrollLock, ct)) return Forbid();
        var user = UserInfo!;
        return Ok(ApiResponse<object>.Ok(await _service.LockAsync(periodId, user.UserId, ct)));
    }

    [HttpGet("periods/{periodId:int}/inputs")]
    public async Task<IActionResult> GetInputs(int periodId, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.PayrollView, ct)) return Forbid();
        return Ok(ApiResponse<object>.Ok(await _service.GetInputsAsync(periodId, ct)));
    }

    [HttpPost("periods/{periodId:int}/export")]
    public async Task<IActionResult> Export(int periodId, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.PayrollExport, ct)) return Forbid();
        var user = UserInfo!;
        var result = await _service.ExportAsync(periodId, user.UserId, ct);
        return File(result.Content, result.ContentType, result.FileName);
    }

    private async Task<bool> CanAsync(int functionCode, CancellationToken ct)
    {
        if (UserInfo == null) return false;
        return await _authorization.HasAsync(UserInfo, functionCode, ct);
    }
}

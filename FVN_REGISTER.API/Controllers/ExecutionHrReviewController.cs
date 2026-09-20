using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Execution;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/execution/hr")]
public sealed class ExecutionHrReviewController : BaseApiController
{
    private readonly IExecutionHrResolutionService _service;

    public ExecutionHrReviewController(
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<ExecutionHrReviewController> logger,
        IOptionsMonitor<AuthDebugOptions> options,
        IExecutionHrResolutionService service)
        : base(currentUser, userLog, logger, options)
    {
        _service = service;
    }

    [HttpGet("reconciliations")]
    public async Task<IActionResult> Get(
        [FromQuery] string? moduleCode,
        [FromQuery] string? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId)
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ."));

        try
        {
            var result = await _service.GetPendingAsync(moduleCode, status, from, to, ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("reconciliations/{reconciliationId:long}/resolve")]
    public async Task<IActionResult> Resolve(
        long reconciliationId,
        [FromBody] ExecutionHrResolutionRequest request,
        CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh người dùng hợp lệ."));

        try
        {
            var result = await _service.ResolveAsync(
                userId,
                UserInfo.EmployeeCode,
                reconciliationId,
                request,
                ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
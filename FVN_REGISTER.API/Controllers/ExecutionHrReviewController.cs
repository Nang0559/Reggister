using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Execution;
using FVN_REGISTER.Application.Interfaces.FeatureOperators;
using FVN_REGISTER.Application.Interfaces.Security;
using FvnAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Exceptions;
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
    private readonly IFeatureOperatorAssignmentService _operators;
    private readonly FvnAuthorizationService _authorization;

    public ExecutionHrReviewController(
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<ExecutionHrReviewController> logger,
        IOptionsMonitor<AuthDebugOptions> options,
        IExecutionHrResolutionService service,
        IFeatureOperatorAssignmentService operators,
        FvnAuthorizationService authorization)
        : base(currentUser, userLog, logger, options)
    {
        _service = service;
        _operators = operators;
        _authorization = authorization;
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
            if (!await CanReviewAsync(ct)) return Forbid();
            var result = await _service.GetPendingAsync(userId, moduleCode, status, from, to, ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("reconciliations/{reconciliationId:long}/detail")]
    public async Task<IActionResult> GetDetail(long reconciliationId, CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ."));

        try
        {
            if (!await CanReviewAsync(ct)) return Forbid();
            var result = await _service.GetDetailAsync(
                userId,
                UserInfo.EmployeeCode,
                reconciliationId,
                ct);

            return result is null
                ? NotFound(ApiResponse<object>.Fail("Không tìm thấy reconciliation."))
                : Ok(ApiResponse<object>.Ok(result));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("evidence/{evidenceId:long}/review")]
    public async Task<IActionResult> ReviewEvidence(
        long evidenceId,
        [FromBody] ExecutionEvidenceReviewRequest request,
        CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh người dùng hợp lệ."));

        try
        {
            if (!await CanReviewAsync(ct)) return Forbid();
            var result = await _service.ReviewEvidenceAsync(
                userId,
                UserInfo.EmployeeCode,
                evidenceId,
                request,
                ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
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
            if (!await CanReviewAsync(ct)) return Forbid();
            var result = await _service.ResolveAsync(
                userId,
                UserInfo.EmployeeCode,
                reconciliationId,
                request,
                ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
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
    private async Task<bool> CanReviewAsync(CancellationToken ct)
    {
        return UserInfo != null
            && await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.ExecutionReview, ct)
            && await _operators.CanOperateAsync(
                UserInfo.UserId,
                UserInfo.EmployeeCode,
                SecurityFunctionCodes.ExecutionReview,
                "EXECUTION_REVIEW",
                null,
                ct);
    }

}

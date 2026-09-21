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
[Route("api/execution")]
public sealed class ExecutionController : BaseApiController
{
    private readonly IExecutionReconciliationService _execution;

    public ExecutionController(
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<ExecutionController> logger,
        IOptionsMonitor<AuthDebugOptions> options,
        IExecutionReconciliationService execution)
        : base(currentUser, userLog, logger, options)
    {
        _execution = execution;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(UserInfo?.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var first = from ?? today.AddDays(-30);
        var last = to ?? today.AddDays(30);

        if (last < first)
            return BadRequest(ApiResponse<object>.Fail("Khoảng ngày không hợp lệ."));

        if (last.DayNumber - first.DayNumber > 93)
            return BadRequest(ApiResponse<object>.Fail("Đối soát chỉ cho phép tối đa 94 ngày mỗi lần tải."));

        var result = await _execution.GetMineAsync(UserInfo.EmployeeCode, first, last, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("me/{reconciliationId:long}")]
    public async Task<IActionResult> Get(long reconciliationId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(UserInfo?.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var result = await _execution.GetAsync(UserInfo.EmployeeCode, reconciliationId, ct);
        return result is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy reconciliation."))
            : Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("me/{reconciliationId:long}/detail")]
    public async Task<IActionResult> GetDetail(long reconciliationId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(UserInfo?.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var result = await _execution.GetDetailAsync(UserInfo.EmployeeCode, reconciliationId, ct);
        return result is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy reconciliation."))
            : Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost("me/{reconciliationId:long}/confirmation")]
    public async Task<IActionResult> SubmitConfirmation(
        long reconciliationId,
        [FromBody] ExecutionConfirmationRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(UserInfo?.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var result = await _execution.SubmitConfirmationAsync(UserInfo.EmployeeCode, reconciliationId, request, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost("me/confirmations/{confirmationId:long}/evidence/upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadEvidence(
        long confirmationId,
        IFormFile file,
        CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ."));

        if (file is null || file.Length <= 0)
            return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn file evidence."));

        await using var stream = file.OpenReadStream();
        var fileId = await _execution.UploadEvidenceFileAsync(
            UserInfo.EmployeeCode,
            userId,
            confirmationId,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            ct);

        return Ok(ApiResponse<int>.Ok(fileId));
    }

    [HttpPost("me/confirmations/{confirmationId:long}/evidence")]
    public async Task<IActionResult> AddEvidence(
        long confirmationId,
        [FromBody] ExecutionEvidenceRequest request,
        CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh người dùng hợp lệ."));

        var result = await _execution.AddEvidenceAsync(UserInfo.EmployeeCode, userId, confirmationId, request, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

}
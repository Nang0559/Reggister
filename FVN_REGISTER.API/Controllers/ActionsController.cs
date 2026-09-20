using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/actions")]
public sealed class ActionsController : BaseApiController
{
    private readonly IActionItemService _actions;

    public ActionsController(
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<ActionsController> logger,
        IOptionsMonitor<AuthDebugOptions> options,
        IActionItemService actions)
        : base(currentUser, userLog, logger, options)
    {
        _actions = actions;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(
        [FromQuery] bool includeCompleted = false,
        CancellationToken ct = default)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var result = await _actions.GetMineAsync(UserInfo.EmployeeCode, userId, includeCompleted, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("me/count")]
    public async Task<IActionResult> GetCount(CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var result = await _actions.GetCountAsync(UserInfo.EmployeeCode, userId, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("me/{actionId:guid}")]
    public async Task<IActionResult> Get(Guid actionId, CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var result = await _actions.GetAsync(UserInfo.EmployeeCode, userId, actionId, ct);
        return result is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy việc cần làm."))
            : Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost("me/{actionId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid actionId, CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var success = await _actions.CompleteAsync(UserInfo.EmployeeCode, userId, actionId, ct);
        return success
            ? Ok(ApiResponse<object>.Ok(new { actionId, status = "Completed" }))
            : BadRequest(ApiResponse<object>.Fail("Không thể hoàn tất việc cần làm."));
    }

    [HttpPost("me/{actionId:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid actionId, CancellationToken ct)
    {
        if (UserInfo?.UserId is not int userId || string.IsNullOrWhiteSpace(UserInfo.EmployeeCode))
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không có định danh nhân viên hợp lệ."));

        var success = await _actions.DismissAsync(UserInfo.EmployeeCode, userId, actionId, ct);
        return success
            ? Ok(ApiResponse<object>.Ok(new { actionId, status = "Dismissed" }))
            : BadRequest(ApiResponse<object>.Fail("Không thể bỏ qua việc cần làm."));
    }
}

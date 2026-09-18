using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.UserManagers;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Contract.Requests.Users;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user-management")]
    public class UserManagementController : BaseApiController
    {
        private readonly IUserManagementService _userMgt;
        private readonly IAuthorizationService _authorization;

        public UserManagementController(
            IUserManagementService userMgt,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<UserManagementController> logger,
            IOptionsMonitor<AuthDebugOptions> options,
            IAuthorizationService authorization)
            : base(currentUser, userLog, logger, options)
        {
            _userMgt = userMgt;
            _authorization = authorization;
        }

        [HttpGet]
        if (!await CanAsync(SecurityFunctionCodes.UserManagementView, ct)) return Forbid();
            public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            return Ok(
                ApiResponse<List<UserAccountDto>>.Ok(
                    await _userMgt.GetAllAsync(ct)));
        }

        [HttpGet("{id:int}")]
        if (!await CanAsync(SecurityFunctionCodes.UserManagementView, ct)) return Forbid();
            public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _userMgt.GetByIdAsync(id, ct);

            if (result == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy."));

            return Ok(ApiResponse<UserAccountDto>.Ok(result));
        }

        [HttpPost]
        if (!await CanAsync(SecurityFunctionCodes.UserManagementCreate, ct)) return Forbid();
            public async Task<IActionResult> Create(public async Task<IActionResult> Create(
            [FromBody] CreateUserRequest request,
            CancellationToken ct)
        {
            return HandleResult(
                await _userMgt.CreateAsync(request, UserInfo!.UserId, ct));
        }

        [HttpPut]
        if (!await CanAsync(SecurityFunctionCodes.UserManagementEdit, ct)) return Forbid();
            public async Task<IActionResult> Update(public async Task<IActionResult> Update(
            [FromBody] UpdateUserRequest request,
            CancellationToken ct)
        {
            return HandleResult(
                await _userMgt.UpdateAsync(request, UserInfo!.UserId, ct));
        }

        [HttpDelete("{id:int}")]
        if (!await CanAsync(SecurityFunctionCodes.UserManagementEdit, ct)) return Forbid();
            public async Task<IActionResult> Delete(public async Task<IActionResult> Delete(
            int id,
            CancellationToken ct)
        {
            if (id == UserInfo!.UserId)
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Không thể xóa tài khoản đang đăng nhập."));

            return HandleResult(
                await _userMgt.DeleteAsync(id, UserInfo.UserId, ct));
        }

        [HttpPut("{id:int}/toggle-lock")]
        if (!await CanAsync(SecurityFunctionCodes.UserManagementLock, ct)) return Forbid();
            public async Task<IActionResult> ToggleLock(public async Task<IActionResult> ToggleLock(
            int id,
            CancellationToken ct)
        {
            if (id == UserInfo!.UserId)
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Không thể khóa tài khoản đang đăng nhập."));

            return HandleResult(
                await _userMgt.ToggleLockAsync(
                    id,
                    UserInfo.UserId,
                    ct));
        }

        [HttpPut("{id:int}/reset-password")]
        if (!await CanAsync(SecurityFunctionCodes.UserManagementResetPassword, ct)) return Forbid();
            public async Task<IActionResult> ResetPassword(public async Task<IActionResult> ResetPassword(
            int id,
            [FromBody] ResetPasswordRequestDto request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Dữ liệu không hợp lệ."));

            return HandleResult(
                await _userMgt.ResetPasswordAsync(
                    id,
                    request.NewPassword,
                    UserInfo!.UserId,
                    ct));
        }
    }

        private async Task<bool> CanAsync(int functionCode, CancellationToken ct)
            => UserInfo != null && await _authorization.HasAsync(UserInfo, functionCode, ct);
    }
}

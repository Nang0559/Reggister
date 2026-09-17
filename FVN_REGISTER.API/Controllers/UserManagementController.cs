using AutoMapper;
using FVN_REGISTER.Application.Interfaces.UserManagers;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Core.Configurations;
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

        public UserManagementController(
            IUserManagementService userMgt,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<UserManagementController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _userMgt = userMgt;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsSuperAdmin()) return Forbid();
            var result = await _userMgt.GetAllAsync(ct);
            return Ok(ApiResponse<List<UserAccountDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsSuperAdmin()) return Forbid();
            var result = await _userMgt.GetByIdAsync(id, ct);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy."));
            return Ok(ApiResponse<UserAccountDto>.Ok(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateUserRequest request,
            CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsSuperAdmin()) return Forbid();
            var result = await _userMgt.CreateAsync(request, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            [FromBody] UpdateUserRequest request,
            CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsSuperAdmin()) return Forbid();
            var result = await _userMgt.UpdateAsync(request, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsSuperAdmin()) return Forbid();
            if (id == UserInfo.UserId)
                return BadRequest(ApiResponse<object>.Fail(
                    "Không thể xóa tài khoản đang đăng nhập."));

            var result = await _userMgt.DeleteAsync(id, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        [HttpPut("{id:int}/toggle-lock")]
        public async Task<IActionResult> ToggleLock(int id, CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsSuperAdmin()) return Forbid();
            if (id == UserInfo.UserId)
                return BadRequest(ApiResponse<object>.Fail(
                    "Không thể khóa tài khoản đang đăng nhập."));

            var result = await _userMgt.ToggleLockAsync(id, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        [HttpPut("{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(
            int id,
            [FromBody] ResetPasswordRequest request,
            CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsSuperAdmin()) return Forbid();
            var result = await _userMgt.ResetPasswordAsync(
                id, request.NewPassword, UserInfo.UserId, ct);
            return HandleResult(result);
        }
    }

    public record ResetPasswordRequest(string NewPassword);
}

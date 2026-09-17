using AutoMapper;
using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.UserManagers;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            ILoggerFactory loggerFactory,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper,
                   loggerFactory.CreateLogger<UserManagementController>(),
                   options)
        {
            _userMgt = userMgt;
        }

        // GET api/user-management
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            if (!UserInfo!.IsSuperAdmin())
                return Forbid();

            var result = await _userMgt.GetAllAsync(ct);
            return Ok(ApiResponse<List<UserAccountDto>>.Ok(result));
        }

        // GET api/user-management/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            if (!UserInfo!.IsSuperAdmin())
                return Forbid();

            var result = await _userMgt.GetByIdAsync(id, ct);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy."));

            return Ok(ApiResponse<UserAccountDto>.Ok(result));
        }

        // POST api/user-management
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateUserRequest request,
            CancellationToken ct)
        {
            if (!UserInfo!.IsSuperAdmin())
                return Forbid();

            var result = await _userMgt.CreateAsync(
                request, UserInfo.UserId, ct);

            return HandleResult(result);
        }

        // PUT api/user-management
        [HttpPut]
        public async Task<IActionResult> Update(
            [FromBody] UpdateUserRequest request,
            CancellationToken ct)
        {
            if (!UserInfo!.IsSuperAdmin())
                return Forbid();

            var result = await _userMgt.UpdateAsync(
                request, UserInfo.UserId, ct);

            return HandleResult(result);
        }

        // DELETE api/user-management/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (!UserInfo!.IsSuperAdmin())
                return Forbid();

            // Không cho tự xóa chính mình
            if (id == UserInfo.UserId)
                return BadRequest(ApiResponse<object>.Fail(
                    "Không thể xóa tài khoản đang đăng nhập."));

            var result = await _userMgt.DeleteAsync(id, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        // PUT api/user-management/{id}/toggle-lock
        [HttpPut("{id:int}/toggle-lock")]
        public async Task<IActionResult> ToggleLock(int id, CancellationToken ct)
        {
            if (!UserInfo!.IsSuperAdmin())
                return Forbid();

            if (id == UserInfo.UserId)
                return BadRequest(ApiResponse<object>.Fail(
                    "Không thể khóa tài khoản đang đăng nhập."));

            var result = await _userMgt.ToggleLockAsync(id, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        // PUT api/user-management/{id}/reset-password
        [HttpPut("{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(
            int id,
            [FromBody] ResetPasswordRequest request,
            CancellationToken ct)
        {
            if (!UserInfo!.IsSuperAdmin())
                return Forbid();

            var result = await _userMgt.ResetPasswordAsync(
                id, request.NewPassword, UserInfo.UserId, ct);

            return HandleResult(result);
        }
    }

    public record ResetPasswordRequest(string NewPassword);
}

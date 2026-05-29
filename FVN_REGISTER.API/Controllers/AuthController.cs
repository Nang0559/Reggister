using AutoMapper;
using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<AuthController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));

            var result = await _authService.Login(model.UserName, model.Password, ct);

            // log chỉ khi success
            if (result.IsSuccess && result.Data != null)
            {
                await _userLog.UpdateLastSeenAsync(
                    result.Data.UserId,
                    "Đăng nhập hệ thống",
                    Path
                );
            }

            return HandleResult(result);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _authService.GetProfileAsync(UserInfo.UserId, ct);

            await LogActionAsync("Truy cập trang cá nhân");

            return HandleResult(result);
        }

        [HttpPut("profile-update")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileRequest request,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy User"));

            var result = await _authService.UpdateProfileAsync(
                UserInfo.UserId,
                request.Email,
                request.AvatarUrl,
                ct);

            await LogActionAsync("Cập nhật profile");

            return HandleResult(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            if (UserInfo == null)
                return Ok(ApiResponse<object>.Ok("Đã đăng xuất"));

            var result = await _authService.Logout(UserInfo.UserId, ct);

            await LogActionAsync("Logout");

            return HandleResult(result);
        }
    }
}

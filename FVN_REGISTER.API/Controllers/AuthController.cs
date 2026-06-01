using AutoMapper;
using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
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

        // ─── LOGIN ───────────────────────────────────────────
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginViewModel model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));

            var result = await _authService.Login(
                model.UserName,
                model.Password,
                model.DeviceId,    // ✅ THÊM
                model.DeviceType,  // ✅ THÊM
                model.DeviceName,  // ✅ THÊM
                model.RememberMe,  // ✅ THÊM
                ct);

            if (result.IsSuccess && result.Data != null)
                await _userLog.UpdateLastSeenAsync(
                    result.Data.UserId,
                    "Đăng nhập hệ thống",
                    Path);

            return HandleResult(result);
        }

        // ─── REFRESH TOKEN ───────────────────────────────────
        [HttpPost("refresh")]                              // ✅ THÊM MỚI
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            CancellationToken ct)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(ApiResponse<object>.Fail("Refresh token không hợp lệ"));

            var result = await _authService.RefreshTokenAsync(
                request.RefreshToken, ct);

            if (!result.IsSuccess)
                return Unauthorized(ApiResponse<object>.Fail(
                    result.Message ?? "Phiên đăng nhập hết hạn"));

            return Ok(ApiResponse<object>.Ok(new { Token = result.Data }));
        }

        // ─── PROFILE ─────────────────────────────────────────
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

        // ─── LOGOUT ──────────────────────────────────────────
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(
            [FromBody] LogoutRequest? request,  // ✅ THÊM body
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Ok(ApiResponse<object>.Ok("Đã đăng xuất"));

            var result = await _authService.Logout(
                UserInfo.UserId,
                request?.RefreshToken,          // ✅ THÊM
                ct);

            await LogActionAsync("Logout");
            return HandleResult(result);
        }

        // ─── SESSIONS ────────────────────────────────────────
        [HttpGet("sessions")]                              // ✅ THÊM MỚI
        public async Task<IActionResult> GetSessions(
            [FromServices] ISessionService sessionService,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized();

            var sessions = await sessionService
                .GetActiveSessionsAsync(UserInfo.UserId, ct);

            return Ok(ApiResponse<List<UserSession>>.Ok(sessions));
        }

        [HttpDelete("sessions/{sessionId}")]               // ✅ THÊM MỚI
        public async Task<IActionResult> RevokeSession(
            int sessionId,
            [FromServices] ISessionService sessionService,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized();

            // Chỉ cho revoke session của chính mình
            var sessions = await sessionService
                .GetActiveSessionsAsync(UserInfo.UserId, ct);

            var target = sessions.FirstOrDefault(x => x.Id == sessionId);
            if (target == null)
                return NotFound(ApiResponse<object>.Fail("Session không tồn tại"));

            await sessionService.RevokeSessionAsync(
                target.RefreshToken, ct);

            return Ok(ApiResponse.Ok("Đã đăng xuất thiết bị"));
        }
    }

    // ─── Request Models ──────────────────────────────────
    public record RefreshTokenRequest(string RefreshToken);
    public record LogoutRequest(string? RefreshToken);
}

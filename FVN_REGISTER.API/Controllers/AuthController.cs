using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests;
using FVN_REGISTER.Contract.Requests.Auths;
using FVN_REGISTER.Contract.Responses;
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
        private readonly ISessionService _sessionService;
        private readonly ITwoFactorService _twoFactorService;

        public AuthController(IAuthService authService, ISessionService sessionService, ITwoFactorService twoFactorService, ICurrentUserService currentUser, IUserLogService userLog, ILogger<AuthController> logger, IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _authService = authService;
            _sessionService = sessionService;
            _twoFactorService = twoFactorService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            var result = await _authService.Login(model.UserName, model.Password, model.DeviceId, model.DeviceType, model.DeviceName, model.RememberMe, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), ct);
            if (result.IsSuccess && result.Data != null && !string.IsNullOrWhiteSpace(result.Data.Token)) await _userLog.UpdateLastSeenAsync(result.Data.UserId, "Đăng nhập hệ thống", Path);
            return HandleResult(result);
        }


        [HttpPost("2fa/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorVerifyRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Mã xác thực không hợp lệ."));
            var result = await _authService.CompleteTwoFactorLoginAsync(
                request.ChallengeToken, request.Code, request.RememberMe,
                HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), ct);
            return HandleResult(result);
        }

        [HttpPost("2fa/setup")]
        [AllowAnonymous]
        public async Task<IActionResult> SetupTwoFactor([FromBody] TwoFactorSetupRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.ChallengeToken)) return BadRequest(ApiResponse<object>.Fail("Thiếu phiên xác thực."));
            return HandleResult(await _twoFactorService.BeginSetupFromChallengeAsync(request.ChallengeToken, ct));
        }

        [HttpPost("2fa/setup-confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmTwoFactorSetup([FromBody] TwoFactorVerifyRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Mã xác thực không hợp lệ."));
            var enabled = await _twoFactorService.ConfirmSetupFromChallengeAsync(request.ChallengeToken, request.Code, ct);
            if (!enabled.IsSuccess) return HandleResult(enabled);
            var login = await _authService.CompleteTwoFactorLoginAsync(
                request.ChallengeToken, request.Code, request.RememberMe,
                HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), ct);
            return HandleResult(login);
        }

        [HttpGet("2fa/status")]
        [Authorize]
        public async Task<IActionResult> TwoFactorStatus(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            return HandleResult(await _twoFactorService.GetStatusAsync(UserInfo.UserId, ct));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Refresh token không hợp lệ"));
            var result = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
            if (!result.IsSuccess) return Unauthorized(ApiResponse<object>.Fail(result.Message ?? "Phiên đăng nhập hết hạn"));
            return Ok(ApiResponse<object>.Ok(new { Token = result.Data }));
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _authService.GetProfileAsync(UserInfo.UserId, ct));
        }

        [HttpPut("profile-update")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommandDto request, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy User"));
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            return HandleResult(await _authService.UpdateProfileAsync(UserInfo.UserId, request.Email, request.AvatarUrl, ct));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu đổi mật khẩu không hợp lệ."));

            var result = await _authService.ChangePassword(
                UserInfo.EmployeeCode??"",
                request.CurrentPassword,
                request.NewPassword,
                ct);

            await LogActionAsync("Đổi mật khẩu");
            return HandleResult(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto? request, CancellationToken ct)
        {
            if (UserInfo == null) return Ok(ApiResponse<object>.Ok("Đã đăng xuất"));
            var result = await _authService.Logout(UserInfo.UserId, request?.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), ct);
            return HandleResult(result);
        }

        [HttpGet("sessions")]
        [Authorize]
        public async Task<IActionResult> GetSessions(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var refreshToken = Request.Headers["X-Refresh-Token"].ToString();
            var sessions = await _sessionService.GetActiveSessionsAsync(UserInfo.UserId, string.IsNullOrWhiteSpace(refreshToken) ? null : refreshToken, ct);
            return Ok(ApiResponse<List<SessionDto>>.Ok(sessions));
        }

        [HttpDelete("sessions/{sessionId:int}")]
        [Authorize]
        public async Task<IActionResult> RevokeSession(int sessionId, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var result = await _sessionService.RevokeSessionAsync(UserInfo.UserId, sessionId, ct);
            if (result == null) return NotFound(ApiResponse<object>.Fail("Thiết bị không tồn tại hoặc đã đăng xuất."));
            return Ok(ApiResponse.Ok("Đã đăng xuất thiết bị thành công."));
        }
    }
}

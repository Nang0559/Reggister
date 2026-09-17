using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Requests.Auths;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Infrastructure.Hubs;
using FVN_REGISTER.Infrastructure.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Core.Configurations;

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
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));

            var result = await _authService.Login(
                model.UserName,
                model.Password,
                model.DeviceId,
                model.DeviceType,
                model.DeviceName,
                model.RememberMe,
                ct);

            if (result.IsSuccess && result.Data != null)
                await _userLog.UpdateLastSeenAsync(
                    result.Data.UserId,
                    "Đăng nhập hệ thống",
                    Path);

            return HandleResult(result);
        }

        [HttpPost("refresh")]
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
            [FromBody] UpdateProfileCommandDto request,
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
        public async Task<IActionResult> Logout(
            [FromBody] LogoutRequest? request,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Ok(ApiResponse<object>.Ok("Đã đăng xuất"));

            var result = await _authService.Logout(
                UserInfo.UserId,
                request?.RefreshToken,
                ct);

            await LogActionAsync("Logout");
            return HandleResult(result);
        }

        // NOTE: Session listing still reads the EF model directly in this transitional step.
        // It is the next extraction target: API -> ISessionService -> Infrastructure repository.
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions(
            [FromServices] FVNWEBAPPContext db,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized();

            var sessions = await db.UserSessions
                .AsNoTracking()
                .Where(x => x.UserId == UserInfo.UserId && x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.DeviceType,
                    x.DeviceName,
                    x.CreatedAt,
                    x.LastSeenAt,
                    IsCurrent = x.JwtToken == Request.Headers["Authorization"].ToString().Replace("Bearer ", "")
                })
                .OrderByDescending(x => x.LastSeenAt)
                .ToListAsync(ct);

            return Ok(ApiResponse<object>.Ok(sessions));
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> RevokeSession(
            int sessionId,
            [FromServices] FVNWEBAPPContext db,
            [FromServices] ISessionService sessionService,
            [FromServices] IHubContext<NotificationHub> hubContext,
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized();

            var targetSession = await db.UserSessions
                .FirstOrDefaultAsync(x =>
                    x.Id == sessionId &&
                    x.UserId == UserInfo.UserId &&
                    x.IsActive,
                    ct);

            if (targetSession == null)
                return NotFound(ApiResponse<object>.Fail("Thiết bị không tồn tại hoặc đã đăng xuất."));

            await sessionService.RevokeAsync(targetSession.JwtToken, ct);

            if (!string.IsNullOrEmpty(targetSession.SignalRConnectionId))
            {
                await hubContext.Clients.Client(targetSession.SignalRConnectionId)
                    .SendAsync("ForceLogout", "Thiết bị của bạn đã bị đăng xuất từ xa.", ct);
            }

            return Ok(ApiResponse.Ok("Đã đăng xuất thiết bị thành công."));
        }
    }

    public record RefreshTokenRequest(string RefreshToken);
    public record LogoutRequest(string? RefreshToken);
}

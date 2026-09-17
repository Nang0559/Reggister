using AutoMapper;
using FVN_REGISTER.API.Hubs;
using FVN_REGISTER.Contract.Dtos.Requests.Auths;
using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Models.Data;
using FVN_REGISTER.Contract.Requests;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
            [FromBody] LoginRequestDto model,
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
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions(
            [FromServices] FVNWEBAPPContext db, // Inject DbContext trực tiếp hoặc qua Service để lấy data nhanh
            CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized();

            // Lấy danh sách session hiển thị cho người dùng, ẩn chuỗi mã JwtToken đi để bảo mật
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
                    // Thêm flag IsCurrent để Front-End biết thiết bị nào là thiết bị hiện tại đang cầm bấm xem
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
    [FromServices] IHubContext<NotificationHub> hubContext, // 🌟 THÊM DÒNG NÀY
    CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized();

            var targetSession = await db.UserSessions
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == UserInfo.UserId && x.IsActive, ct);

            if (targetSession == null)
                return NotFound(ApiResponse<object>.Fail("Thiết bị không tồn tại hoặc đã đăng xuất."));

            await sessionService.RevokeAsync(targetSession.JwtToken, ct);

            // 🌟 ĐỔI chữ _hubContext thành hubContext (bỏ dấu gạch dưới vì dùng biến local)
            if (!string.IsNullOrEmpty(targetSession.SignalRConnectionId))
            {
                await hubContext.Clients.Client(targetSession.SignalRConnectionId)
                    .SendAsync("ForceLogout", "Thiết bị của bạn đã bị đăng xuất từ xa.", ct);
            }

            return Ok(ApiResponse.Ok("Đã đăng xuất thiết bị thành công."));
        }
    }

    // ─── Request Models ──────────────────────────────────
    public record RefreshTokenRequest(string RefreshToken);
    public record LogoutRequest(string? RefreshToken);
}

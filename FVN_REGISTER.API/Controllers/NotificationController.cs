using AutoMapper;
using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseApiController
    {
        private readonly INotificationService _notification;

        public NotificationController(
            INotificationService notification,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<NotificationController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _notification = notification;
        }

        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1, CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized();
            var list = await _notification.GetByUserAsync(UserInfo.UserId, page, 20, ct);
            return Ok(ApiResponse<object>.Ok(list));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized();
            var count = await _notification.GetUnreadCountAsync(UserInfo.UserId, ct);
            return Ok(ApiResponse<object>.Ok(new { count }));
        }

        // ✅ Đổi [HttpPatch] → [HttpPut] để dùng được IHttpClientWithAuth.PutAsync
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id, CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized();
            var result = await _notification.MarkReadAsync(id, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        // ✅ Đổi [HttpPatch] → [HttpPut]
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized();
            var result = await _notification.MarkAllReadAsync(UserInfo.UserId, ct);
            return HandleResult(result);
        }
    }
}

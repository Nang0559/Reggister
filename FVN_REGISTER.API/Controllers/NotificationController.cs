using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IApprovalInboxService _approvalInbox;

        public NotificationController(
            INotificationService notification,
            IApprovalInboxService approvalInbox,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<NotificationController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _notification = notification;
            _approvalInbox = approvalInbox;
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, CancellationToken ct = default)
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

        [HttpGet("action-center")]
        public async Task<IActionResult> ActionCenter(CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized();

            var notifications = await _notification.GetByUserAsync(UserInfo.UserId, 1, 8, ct);
            var unread = await _notification.GetUnreadCountAsync(UserInfo.UserId, ct);
            var approvalResult = await _approvalInbox.GetPendingAsync(UserInfo, ct);

            var groups = approvalResult.Success
                ? approvalResult.Data ?? new List<FVN_REGISTER.Contract.Dtos.Approvals.PendingApprovalGroupDto>()
                : new List<FVN_REGISTER.Contract.Dtos.Approvals.PendingApprovalGroupDto>();

            var result = new ActionCenterDto
            {
                UnreadNotificationCount = unread,
                PendingApprovalCount = groups.Sum(x => x.Count),
                Notifications = notifications,
                ApprovalGroups = groups
            };

            return Ok(ApiResponse<ActionCenterDto>.Ok(result));
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id, CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized();
            return HandleResult(await _notification.MarkReadAsync(id, UserInfo.UserId, ct));
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized();
            return HandleResult(await _notification.MarkAllReadAsync(UserInfo.UserId, ct));
        }
    }
}

using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Interfaces.Auths;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace FVN_REGISTER.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly INotificationService _notify;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(
            INotificationService notify,
            ILogger<NotificationHub> logger)
        {
            _notify = notify;
            _logger = logger;
        }

        // ─── Connection ──────────────────────────────────────────────
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

                // Gửi badge count ngay khi connect
                var unread = await _notify.GetUnreadCountAsync(userId.Value);
                await Clients.Caller.SendAsync("BadgeUpdated", unread);

                _logger.LogInformation(
                    "[HUB] Connected UserId={UserId} ConnId={ConnId}",
                    userId, Context.ConnectionId);
            }
            else
            {
                _logger.LogWarning(
                    "[HUB] No UserId claim. ConnId={ConnId}", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId.HasValue)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

            await base.OnDisconnectedAsync(exception);
        }

        // ─── Methods client gọi ──────────────────────────────────────

        /// <summary>
        /// Client: await _hub.InvokeAsync&lt;IEnumerable&lt;NotificationDto&gt;&gt;("GetNotifications")
        /// </summary>
        public async Task<IEnumerable<NotificationDto>> GetNotifications()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Enumerable.Empty<NotificationDto>();

            return await _notify.GetByUserAsync(userId.Value, page: 1, pageSize: 30);
        }

        /// <summary>
        /// Client: await _hub.InvokeAsync("MarkRead", id)
        /// </summary>
        public async Task MarkRead(int notificationId)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return;

            await _notify.MarkReadAsync(notificationId, userId.Value);
            // BadgeUpdated đã được push bên trong MarkReadAsync
        }

        /// <summary>
        /// Client: await _hub.InvokeAsync("MarkAllRead")
        /// </summary>
        public async Task MarkAllRead()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return;

            await _notify.MarkAllReadAsync(userId.Value);
            // BadgeUpdated đã được push bên trong MarkAllReadAsync
        }

        // ─── Helper ──────────────────────────────────────────────────
        private int? GetUserId()
        {
            var raw = Context.User?.FindFirst("UserId")?.Value
                   ?? Context.User?.FindFirst("nameid")?.Value;
            return int.TryParse(raw, out var id) ? id : null;
        }
    }

}

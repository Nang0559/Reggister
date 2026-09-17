using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly INotificationService _notify;
        private readonly ILogger<NotificationHub> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public NotificationHub(
            INotificationService notify,
            ILogger<NotificationHub> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _notify = notify;
            _logger = logger;
            _options = options;
        }

        // ─── Connection ──────────────────────────────────────────────

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();

            if (userId.HasValue)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId, NotificationHubGroups.ForUser(userId.Value));

                var unread = await _notify.GetUnreadCountAsync(userId.Value);
                await Clients.Caller.SendAsync("BadgeUpdated", unread);

                _logger.LogInfoIf(Debug,
                    "[HUB] Connected | UserId={UserId} | ConnId={ConnId} | Unread={Unread}",
                    userId, Context.ConnectionId, unread);
            }
            else
            {
                // Không có UserId → luôn warn, không cần flag
                _logger.LogWarning(
                    "[HUB] Connected but no UserId claim | ConnId={ConnId}",
                    Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();

            if (userId.HasValue)
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId, NotificationHubGroups.ForUser(userId.Value)) ;

                _logger.LogInfoIf(Debug,
                    "[HUB] Disconnected | UserId={UserId} | ConnId={ConnId}",
                    userId, Context.ConnectionId);
            }

            if (exception != null)
            {
                // Lỗi disconnect → luôn log error
                _logger.LogError(exception,
                    "[HUB] Disconnected with error | ConnId={ConnId}",
                    Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ─── Methods client gọi ──────────────────────────────────────

        /// <summary>
        /// Client: await _hub.InvokeAsync&lt;IEnumerable&lt;NotificationDto&gt;&gt;("GetNotifications")
        /// </summary>
        public async Task<IEnumerable<NotificationDto>> GetNotifications()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning(
                    "[HUB] GetNotifications → no userId | ConnId={ConnId}",
                    Context.ConnectionId);
                return Enumerable.Empty<NotificationDto>();
            }

            var list = await _notify.GetByUserAsync(userId.Value, page: 1, pageSize: 30);

            _logger.LogDebugIf(Debug,
                "[HUB] GetNotifications | UserId={UserId} | Count={Count}",
                userId, list.Count);

            return list;
        }

        /// <summary>
        /// Client: await _hub.InvokeAsync("MarkRead", id)
        /// </summary>
        public async Task<bool> MarkRead(int notificationId)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return false;

            var result = await _notify.MarkReadAsync(notificationId, userId.Value);

            _logger.LogDebugIf(Debug,
                "[HUB] MarkRead | UserId={UserId} | NotificationId={NotifId} | Success={Success}",
                userId, notificationId, result.IsSuccess);

            return result.IsSuccess;
        }

        /// <summary>
        /// Client: await _hub.InvokeAsync("MarkAllRead")
        /// </summary>
        public async Task<bool> MarkAllRead()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return false;

            var result = await _notify.MarkAllReadAsync(userId.Value);

            _logger.LogDebugIf(Debug,
                "[HUB] MarkAllRead | UserId={UserId}",
                userId);
            return result.IsSuccess;
        }

        // ─── Helper ──────────────────────────────────────────────────

        private int? GetUserId()
        {
            var raw = Context.User?.FindFirst("UserId")?.Value
                   ?? Context.User?.FindFirst("nameid")?.Value;

            _logger.LogDebugIf(Debug,
                "[HUB] GetUserId | raw={Raw} | ConnId={ConnId}",
                raw, Context.ConnectionId);

            // Claims chỉ log khi debug bật
            if (Debug)
            {
                var claims = string.Join(" | ",
                    Context.User?.Claims.Select(c => $"{c.Type}={c.Value}")
                    ?? Array.Empty<string>());

                _logger.LogDebugIf(Debug,
                    "[HUB] Claims | {Claims}", claims);
            }

            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
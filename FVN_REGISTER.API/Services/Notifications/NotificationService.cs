using FVN_REGISTER.API.Hubs;
using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.API.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            FVNWEBAPPContext db,
            IHubContext<NotificationHub> hub,
            ILogger<NotificationService> logger)
        {
            _db = db;
            _hub = hub;
            _logger = logger;
        }

        public async Task SendAsync(int userId, string title, string body,
            string type, int? refId = null, CancellationToken ct = default)
        {
            // 1. Lưu vào DB
            var notification = new AppNotification
            {
                UserId = userId,
                Title = title,
                Body = body,
                Type = type,
                RefId = refId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _db.AppNotifications.Add(notification);
            await _db.SaveChangesAsync(ct);

            // 2. Push realtime qua SignalR
            var unreadCount = await GetUnreadCountAsync(userId, ct);

            await _hub.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title,
                    body,
                    type,
                    refId,
                    createdAt = notification.CreatedAt,
                    unreadCount
                }, ct);

            _logger.LogInformation(
                "[NOTIFY] Sent to UserId={UserId} Type={Type}", userId, type);
        }

        public async Task<List<AppNotification>> GetUnreadAsync(
            int userId, CancellationToken ct = default)
            => await _db.AppNotifications
                .Where(x => x.UserId == userId && !x.IsRead==true)
                .OrderByDescending(x => x.CreatedAt)
                .Take(20)
                .ToListAsync(ct);

        public async Task<int> GetUnreadCountAsync(
            int userId, CancellationToken ct = default)
            => await _db.AppNotifications
                .CountAsync(x => x.UserId == userId && !x.IsRead==true, ct);

        public async Task MarkReadAsync(int id, CancellationToken ct = default)
        {
            var n = await _db.AppNotifications.FindAsync([id], ct);
            if (n == null) return;
            n.IsRead = true;
            n.ReadAt = DateTime.Now;
            await _db.SaveChangesAsync(ct);
        }

        public async Task MarkAllReadAsync(int userId, CancellationToken ct = default)
        {
            await _db.AppNotifications
                .Where(x => x.UserId == userId && !x.IsRead == true)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsRead, true)
                    .SetProperty(x => x.ReadAt, DateTime.Now), ct);
        }
    }
}

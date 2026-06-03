using FVN_REGISTER.API.Hubs;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Core.Logging;


namespace FVN_REGISTER.API.Services.Notifications
{
  
    public class NotificationService
        : BaseService<NotificationService>, INotificationService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(
            FVNWEBAPPContext db,
            IHubContext<NotificationHub> hub,
            ILogger<NotificationService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
            _hub = hub;
        }

        // ================= CREATE =================
        public async Task CreateAsync(CreateNotificationDto dto, CancellationToken ct = default)
        {
            try
            {
                var entity = new AppNotification
                {
                    UserId = dto.UserId,
                    EmployeeCode = dto.EmployeeCode,
                    NotificationType = dto.NotificationType,
                    Title = dto.Title,
                    Body = dto.Body,
                    ActionUrl = dto.ActionUrl,
                    RelatedLeaveId = dto.RelatedLeaveId,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _db.AppNotifications.Add(entity);
                await _db.SaveChangesAsync(ct);

                Logger.LogDebugIf(Debug,
                    "[NOTIFY] Created for UserId={UserId} Type={Type}",
                    dto.UserId, dto.NotificationType);

                // Tự động push real-time qua SignalR sau khi lưu DB thành công
                await PushToUserAsync(dto.UserId, entity, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[NOTIFY] CreateAsync error for UserId={UserId}", dto.UserId);
            }
        }

        // ✅ ĐỒNG BỘ: Hàm tương thích ngược với code cũ của đơn nghỉ phép
        public async Task NotifyPendingLeaveAsync(int approverUserId, string employeeName, int leaveId, CancellationToken ct = default)
        {
            var dto = new CreateNotificationDto
            {
                UserId = approverUserId,
                NotificationType = "LEAVE_PENDING",
                Title = "Đơn nghỉ phép mới",
                Body = $"{employeeName} vừa gửi đơn nghỉ phép cần phê duyệt",
                ActionUrl = $"/approve/list?id={leaveId}",
                RelatedLeaveId = leaveId
            };

            await CreateAsync(dto, ct);
        }

        // ================= QUERY =================
        public async Task<List<NotificationDto>> GetByUserAsync(
            int userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            return await _db.AppNotifications
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NotificationDto
                {
                    Id = x.Id,
                    NotificationType = x.NotificationType,
                    Title = x.Title,
                    Body = x.Body,
                    ActionUrl = x.ActionUrl,
                    RelatedLeaveId = x.RelatedLeaveId,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(ct);
        }

        public async Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default)
            => await _db.AppNotifications
                .CountAsync(x => x.UserId == userId && !x.IsRead, ct);

        // ================= MARK READ =================
        public async Task<ServiceResult> MarkReadAsync(
            int notificationId, int userId, CancellationToken ct = default)
        {
            var entity = await _db.AppNotifications
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, ct);

            if (entity == null)
                return ServiceResult.Fail("Không tìm thấy thông báo.");

            entity.IsRead = true;
            entity.ReadAt = DateTime.Now;
            await _db.SaveChangesAsync(ct);

            var unread = await GetUnreadCountAsync(userId, ct);
            await _hub.Clients.Group($"user_{userId}")
                .SendAsync("BadgeUpdated", unread, cancellationToken: ct);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> MarkAllReadAsync(int userId, CancellationToken ct = default)
        {
            await _db.AppNotifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsRead, true)
                    .SetProperty(x => x.ReadAt, DateTime.Now), ct);

            await _hub.Clients.Group($"user_{userId}")
                .SendAsync("BadgeUpdated", 0, cancellationToken: ct);

            return ServiceResult.Ok("Đã đánh dấu tất cả đã đọc.");
        }

        // ================= REALTIME PUSH (SIGNALR) =================
        public async Task PushToUserAsync(
            int userId, AppNotification entity, CancellationToken ct)
        {
            try
            {
                var unread = await GetUnreadCountAsync(userId, ct);

                var payload = new NotificationPushDto
                {
                    UnreadCount = unread,
                    Latest = new NotificationDto
                    {
                        Id = entity.Id,
                        NotificationType = entity.NotificationType,
                        Title = entity.Title,
                        Body = entity.Body,
                        ActionUrl = entity.ActionUrl,
                        RelatedLeaveId = entity.RelatedLeaveId,
                        IsRead = false,
                        CreatedAt = entity.CreatedAt
                    }
                };

                // ✅ Gửi gộp payload thay vì bắn 2 sự kiện riêng biệt làm lag client Blazor/Mobile
                // Client chỉ cần lắng nghe "ReceiveNotification" và lấy luôn UnreadCount từ payload để update badge
                await _hub.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", payload, cancellationToken: ct);

                // Vẫn giữ sự kiện cập nhật Badge rời nếu client của bạn có logic xử lý riêng
                await _hub.Clients.Group($"user_{userId}")
                    .SendAsync("BadgeUpdated", unread, cancellationToken: ct);

                Logger.LogDebugIf(Debug,
                    "[NOTIFY] Pushed to user_{UserId} | Unread={Count}",
                    userId, unread);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[NOTIFY] SignalR push failed for UserId={UserId}", userId);
            }
        }
    }
}

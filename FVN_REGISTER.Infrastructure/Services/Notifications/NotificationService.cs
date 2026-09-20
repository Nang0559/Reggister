using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Infrastructure.Utils;


namespace FVN_REGISTER.Infrastructure.Services.Notifications
{
    public class NotificationService : BaseService<NotificationService>, INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(
            IUnitOfWork uow,
            IHubContext<NotificationHub> hub,
            ILogger<NotificationService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
            _hub = hub;
        }

        public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto, CancellationToken ct = default)
        {
            var entity = new F03AppNotification
            {
                UserId = dto.UserId,
                EmployeeCode = dto.EmployeeCode,
                RequestModule = dto.Module,
                Action = dto.Action,
                Title = dto.Title,
                Body = dto.Body,
                ActionUrl = dto.ActionUrl,
                ApprovalLevel = dto.ApprovalLevel,
                RelatedLeaveId = dto.Module == RequestModule.Leave ? dto.RelatedRequestId : null,
                RelatedOTId = dto.Module == RequestModule.Overtime ? dto.RelatedRequestId : null,
                IsHighPriority = dto.IsHighPriority,
                Metadata = dto.Metadata,
                ActionId = dto.ActionId,
                IsRead = false
            };

            await _uow.Repository<F03AppNotification>().AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            // SỬA: map sang DTO trước khi dùng tiếp — 1 nguồn map duy nhất, dùng lại cho cả return và push
            var resultDto = NotificationMapper.ToDto(entity);

            await PushRealtimeAsync(dto.UserId, resultDto, ct);

            Logger.LogDebugIf(Debug, "[NOTIFY] Created for UserId={UserId} | {Title}", dto.UserId, dto.Title);

            return resultDto;   // SỬA: trả DTO, không trả entity
        }

        public async Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default)
        {
            return await _uow.Repository<F03AppNotification>()
                .Query().AsNoTracking()
                .CountAsync(x => x.UserId == userId && !x.IsRead, ct);
        }

        public async Task<List<NotificationDto>> GetByUserAsync(
    int userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            return await _uow.Repository<F03AppNotification>()
                .Query().AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NotificationDto
                {
                    Id = x.Id,
                    Module = x.RequestModule,
                    Action = x.Action,
                    Title = x.Title,
                    Body = x.Body ?? string.Empty,
                    ApprovalLevel = x.ApprovalLevel,
                    ActionUrl = x.ActionUrl ?? string.Empty,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt,
                    ActionId = x.ActionId
                })
                .ToListAsync(ct);
        }

        public async Task<ServiceResult> MarkReadAsync(int notificationId, int userId, CancellationToken ct = default)
        {
            var entity = await _uow.Repository<F03AppNotification>()
                .Query()
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, ct);

            if (entity == null)
                return ServiceResult.Fail("Không tìm thấy thông báo.");

            if (!entity.IsRead)
            {
                entity.IsRead = true;
                entity.ReadAt = DateTime.Now;
                _uow.Repository<F03AppNotification>().Update(entity);
                await _uow.SaveChangesAsync(ct);
            }

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> MarkAllReadAsync(int userId, CancellationToken ct = default)
        {
            var unread = await _uow.Repository<F03AppNotification>()
                .Query()
                .Where(x => x.UserId == userId && !x.IsRead)
                .ToListAsync(ct);

            if (unread.Count == 0)
                return ServiceResult.Ok("Không có thông báo nào cần đánh dấu.");

            var now = DateTime.Now;
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = now;
            }

            await _uow.SaveChangesAsync(ct);
            return ServiceResult.Ok();
        }

        // private helper — không lộ ra interface
        private async Task PushRealtimeAsync(int userId, NotificationDto dto, CancellationToken ct)
        {
            await _hub.Clients.Group(NotificationHubGroups.ForUser(userId))
                .SendAsync("ReceiveNotification", dto, ct);
        }

    }
}

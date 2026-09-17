using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Application.Interfaces.Notifications
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateAsync(CreateNotificationDto dto, CancellationToken ct = default);

        Task<List<NotificationDto>> GetByUserAsync(
            int userId,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default);

        Task<ServiceResult> MarkReadAsync(int notificationId, int userId, CancellationToken ct = default);

        Task<ServiceResult> MarkAllReadAsync(int userId, CancellationToken ct = default);
    }
}

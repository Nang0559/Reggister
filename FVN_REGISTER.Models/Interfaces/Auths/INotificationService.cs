using FVN_REGISTER.Contract.Models;


namespace FVN_REGISTER.Contract.Interfaces.Auths
{
    public interface INotificationService
    {
        Task SendAsync(int userId, string title, string body,
                       string type, int? refId = null,
                       CancellationToken ct = default);

        Task<List<AppNotification>> GetUnreadAsync(int userId,
                                                   CancellationToken ct = default);

        Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default);

        Task MarkReadAsync(int notificationId, CancellationToken ct = default);

        Task MarkAllReadAsync(int userId, CancellationToken ct = default);
    }
}

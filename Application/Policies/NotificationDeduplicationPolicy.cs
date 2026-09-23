using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Core.Entities.Common;

namespace FVN_REGISTER.Application.Policies;

public static class NotificationDeduplicationPolicy
{
    public static bool IsDuplicate(
        F03AppNotification existing,
        CreateNotificationDto requested)
        => existing.IsActive != false
            && existing.UserId == requested.UserId
            && existing.ActionId.HasValue
            && existing.ActionId == requested.ActionId
            && !existing.IsRead
            && !string.IsNullOrWhiteSpace(existing.NotificationType)
            && string.Equals(
                existing.NotificationType,
                requested.NotificationType,
                StringComparison.OrdinalIgnoreCase);
}

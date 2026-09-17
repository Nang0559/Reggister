
using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Factories
{
    public interface INotificationFactory
    {
        CreateNotificationDto CreateApprovalNotification(
            int userId,
            string? employeeCode,
            RequestModule module,
            int requestId,
            NotificationAction action,
            int? level = null);
    }
}

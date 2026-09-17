using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Core.Entities.Common;


namespace FVN_REGISTER.Application.Maps
{
    public static class NotificationMapper
    {
        public static NotificationDto ToDto(F03AppNotification entity) => new()
        {
            Id = entity.Id,
            Module = entity.RequestModule,
            Action = entity.Action,
            Title = entity.Title,
            Body = entity.Body,
            ApprovalLevel = entity.ApprovalLevel,
            ActionUrl = entity.ActionUrl,
            IsRead = entity.IsRead,
            CreatedAt = entity.CreatedAt
        };
    }
}

using FVN_REGISTER.Application.Policies;
using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Enums;
using Xunit;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class NotificationDeduplicationPolicyTests
{
    [Fact]
    public void Same_unread_action_notification_is_duplicate()
    {
        var actionId = Guid.NewGuid();
        var existing = new F03AppNotification
        {
            UserId = 7,
            ActionId = actionId,
            NotificationType = "EXECUTION_ACTION",
            IsRead = false,
            IsActive = true
        };

        var requested = new CreateNotificationDto
        {
            UserId = 7,
            Module = RequestModule.Overtime,
            Action = NotificationAction.Pending,
            Title = "Same action",
            ActionId = actionId,
            NotificationType = "execution_action"
        };

        Assert.True(NotificationDeduplicationPolicy.IsDuplicate(existing, requested));
    }

    [Fact]
    public void Read_or_different_action_notification_is_not_duplicate()
    {
        var existing = new F03AppNotification
        {
            UserId = 7,
            ActionId = Guid.NewGuid(),
            NotificationType = "EXECUTION_ACTION",
            IsRead = true,
            IsActive = true
        };

        var requested = new CreateNotificationDto
        {
            UserId = 7,
            Module = RequestModule.Overtime,
            Action = NotificationAction.Pending,
            Title = "Same action",
            ActionId = Guid.NewGuid(),
            NotificationType = "EXECUTION_ACTION"
        };

        Assert.False(NotificationDeduplicationPolicy.IsDuplicate(existing, requested));
    }
}

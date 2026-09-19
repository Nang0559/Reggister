using FVN_REGISTER.Contract.Dtos.Approvals;

namespace FVN_REGISTER.Contract.Dtos.Notifications;

public sealed class ActionCenterDto
{
    public int UnreadNotificationCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public List<NotificationDto> Notifications { get; set; } = new();
    public List<PendingApprovalGroupDto> ApprovalGroups { get; set; } = new();
}

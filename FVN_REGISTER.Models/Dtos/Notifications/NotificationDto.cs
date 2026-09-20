


using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Notifications
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public RequestModule Module { get; set; }
        public NotificationAction Action { get; set; }
        public int? ApprovalLevel { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ActionId { get; set; }
    }
}

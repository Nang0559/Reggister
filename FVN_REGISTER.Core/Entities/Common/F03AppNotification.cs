using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common
{
    [Table("F03AppNotifications")]
    public partial class F03AppNotification : BaseAuditEntity
    {
        [Required]
        public int UserId { get; set; }

        [StringLength(50)]
        public string? EmployeeCode { get; set; }

        [Required]
        public RequestModule RequestModule { get; set; }

        [Required]
        public NotificationAction Action { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Body { get; set; }

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        public int? RelatedLeaveId { get; set; }
        public int? RelatedOTId { get; set; }
        public int? ApprovalLevel { get; set; }
        public bool IsHighPriority { get; set; } = false;

        [StringLength(2000)]
        public string? Metadata { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // Shared Action contract. Legacy notifications may keep this null.
        public Guid? ActionId { get; set; }

        [StringLength(50)]
        public string? NotificationType { get; set; }
    }
}

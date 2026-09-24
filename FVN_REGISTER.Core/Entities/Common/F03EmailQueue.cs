using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Common;

[Table("F03EmailQueues")]
public partial class F03EmailQueue : BaseAuditEntity
{
    [Required, StringLength(255)]
    public string ToEmail { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [StringLength(50)]
    public string? TemplateCode { get; set; }

    [StringLength(20)]
    public string? EmailProfileCode { get; set; }

    public EmailDispatchMode DispatchMode { get; set; } = EmailDispatchMode.AutoSend;

    public string? Payload { get; set; }

    public EmailStatus Status { get; set; } = EmailStatus.Pending;

    public int RetryCount { get; set; }
    public int MaxRetry { get; set; } = 3;

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    public DateTime? SentAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledBy { get; set; }
}

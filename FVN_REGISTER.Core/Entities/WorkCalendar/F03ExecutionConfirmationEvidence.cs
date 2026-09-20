using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

[Table("F03ExecutionConfirmationEvidence")]
public sealed class F03ExecutionConfirmationEvidence : BaseAuditEntity
{
    public long Id { get; set; }

    public long ConfirmationId { get; set; }

    [Required, StringLength(50)]
    public string EvidenceType { get; set; } = string.Empty;

    public int? FileId { get; set; }

    [StringLength(200)]
    public string? ReferenceNo { get; set; }

    [StringLength(1000)]
    public string? ExternalUrl { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public int? SubmittedBy { get; set; }
    public DateTime SubmittedAt { get; set; }

    [Required, StringLength(50)]
    public string ReviewStatus { get; set; } = "Pending";

    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    [StringLength(2000)]
    public string? ReviewNote { get; set; }
}

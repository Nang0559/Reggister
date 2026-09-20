using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

[Table("F03ExecutionConfirmations")]
public sealed class F03ExecutionConfirmation : BaseAuditEntity
{
    public long Id { get; set; }

    public long ReconciliationId { get; set; }

    [Required, StringLength(50)]
    public string ModuleCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string SourceId { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string SourceType { get; set; } = "MODULE";

    [StringLength(100)]
    public string? ParticipantId { get; set; }

    public int EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }

    [StringLength(50)]
    public string? Decision { get; set; }

    [Required, StringLength(50)]
    public string Status { get; set; } = "Pending";

    [StringLength(2000)]
    public string? Comment { get; set; }

    public bool EvidenceRequired { get; set; }

    public DateTime? SubmittedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    [StringLength(2000)]
    public string? ReviewNote { get; set; }
}

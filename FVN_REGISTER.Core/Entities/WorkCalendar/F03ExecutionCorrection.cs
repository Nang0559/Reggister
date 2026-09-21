using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

[Table("F03ExecutionCorrections")]
public sealed class F03ExecutionCorrection : BaseAuditEntity
{
    public new long Id { get; set; }
    public long ReconciliationId { get; set; }
    public long ResolutionId { get; set; }

    [Required, StringLength(50)]
    public string ModuleCode { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string CorrectionType { get; set; } = string.Empty;

    public int EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "Pending";

    [StringLength(100)]
    public string? RequestedState { get; set; }

    [StringLength(100)]
    public string? AppliedState { get; set; }

    public DateTime? AppliedAt { get; set; }
    public int? AppliedBy { get; set; }

    [StringLength(2000)]
    public string? Reason { get; set; }
}
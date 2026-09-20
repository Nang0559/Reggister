using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

[Table("F03ExecutionReconciliationHistory")]
public sealed class F03ExecutionReconciliationHistory
{
    public long Id { get; set; }
    public long ReconciliationId { get; set; }

    [StringLength(50)]
    public string? FromStatus { get; set; }

    [Required, StringLength(50)]
    public string ToStatus { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Reason { get; set; }

    public int? ActorUserId { get; set; }
    public int? ActorEmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

[Table("F03ExecutionResolutions")]
public sealed class F03ExecutionResolution : BaseAuditEntity
{
    public new long Id { get; set; }
    public long ReconciliationId { get; set; }

    [Required, StringLength(20)]
    public string Decision { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Reason { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string CalendarAction { get; set; } = "KEEP";

    public int ResolvedByUserId { get; set; }
    public int ResolvedByEmployeeId { get; set; }
    public DateTime ResolvedAt { get; set; }
    public string? AppliedChangeJson { get; set; }
}
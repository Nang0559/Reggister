using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

[Table("F03ActionPolicies")]
public sealed class F03ActionPolicy : BaseAuditEntity
{
    [Required, StringLength(50)]
    public string ModuleCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string ActionType { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
    public int DefaultPriority { get; set; } = 100;
    public int? DueHours { get; set; }
    public bool NotificationEnabled { get; set; } = true;
    public bool EscalationEnabled { get; set; }

    [StringLength(200)]
    public string? TitleTemplate { get; set; }

    [StringLength(1000)]
    public string? SummaryTemplate { get; set; }
}

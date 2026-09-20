using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

[Table("F03CalendarModuleDefinitions")]
public sealed class F03CalendarModuleDefinition : BaseAuditEntity
{
    [Required, StringLength(50)]
    public string ModuleCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string ModuleName { get; set; } = string.Empty;

    public bool SupportsCalendar { get; set; } = true;
    public bool DefaultEnabled { get; set; } = true;
}

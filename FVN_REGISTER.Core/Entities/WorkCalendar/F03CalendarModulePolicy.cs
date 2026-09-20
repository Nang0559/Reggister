using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

[Table("F03CalendarModulePolicies")]
public sealed class F03CalendarModulePolicy : BaseAuditEntity
{
    [Required, StringLength(50)]
    public string ModuleCode { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
    public CalendarDisplayMode DisplayMode { get; set; } = CalendarDisplayMode.MarkerAndSummary;
    public CalendarNoteMode NoteMode { get; set; } = CalendarNoteMode.AlertsOnly;
    public CalendarConfirmationMode ConfirmationMode { get; set; } = CalendarConfirmationMode.None;
    public CalendarReconciliationMode ReconciliationMode { get; set; } = CalendarReconciliationMode.None;
    public int Priority { get; set; } = 100;

    [StringLength(500)]
    public string? SummaryTemplate { get; set; }

    [StringLength(500)]
    public string? DetailTemplate { get; set; }

    public int? UpdatedBy { get; set; }
}

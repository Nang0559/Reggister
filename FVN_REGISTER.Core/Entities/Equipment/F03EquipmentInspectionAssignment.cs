using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentInspectionAssignments")]
public sealed class F03EquipmentInspectionAssignment : BaseAuditEntity
{
    public int EquipmentId { get; set; }
    public int TemplateId { get; set; }
    [Required, StringLength(20)] public string Frequency { get; set; } = "Daily";
    public TimeSpan DueTime { get; set; } = new(8, 0, 0);
    public int ReminderHoursBefore { get; set; } = 24;
    public int? ScheduleDayOfWeek { get; set; }
    public int? ScheduleDayOfMonth { get; set; }
    public int? ScheduleMonth { get; set; }
    [Required, StringLength(50)] public string InspectorEmployeeCode { get; set; } = string.Empty;
    [Required, StringLength(50)] public string ApproverEmployeeCode { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; } = DateTime.Today;
    public DateTime? EffectiveTo { get; set; }
    public F03EquipmentAsset? Equipment { get; set; }
    public F03EquipmentInspectionTemplate? Template { get; set; }
}
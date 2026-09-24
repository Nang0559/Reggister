using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentInspectionTasks")]
public sealed class F03EquipmentInspectionTask : BaseAuditEntity
{
    public int EquipmentId { get; set; }
    public int TemplateId { get; set; }
    public int AssignmentId { get; set; }
    /// <summary>Shared Action Center identity for Dashboard / Work Inbox / Notification / Calendar.</summary>
    public Guid? ActionId { get; set; }
    public Guid? ApprovalActionId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime DueAt { get; set; }
    [Required, StringLength(30)] public string Status { get; set; } = "Scheduled";
    [StringLength(30)] public string? Result { get; set; }
    [StringLength(50)] public string InspectorEmployeeCode { get; set; } = string.Empty;
    [StringLength(50)] public string ApproverEmployeeCode { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? ReminderSentAt { get; set; }
    public DateTime? OverdueReminderSentAt { get; set; }
    [StringLength(1000)] public string? RejectReason { get; set; }
    public F03EquipmentAsset? Equipment { get; set; }
    public F03EquipmentInspectionTemplate? Template { get; set; }
    public ICollection<F03EquipmentInspectionItemResult> ItemResults { get; set; } = new List<F03EquipmentInspectionItemResult>();
    public ICollection<F03EquipmentInspectionEvidence> Evidence { get; set; } = new List<F03EquipmentInspectionEvidence>();
}
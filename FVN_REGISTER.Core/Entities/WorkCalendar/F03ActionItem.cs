using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

[Table("F03ActionItems")]
public sealed class F03ActionItem : BaseAuditEntity
{
    public Guid ActionId { get; set; }

    [Required, StringLength(50)]
    public string ModuleCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string SourceId { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string SourceType { get; set; } = "MODULE";

    [StringLength(100)]
    public string? ParticipantId { get; set; }

    public int EmployeeId { get; set; }
    public int? AssignedToUserId { get; set; }
    public int AssignedToEmployeeId { get; set; }
    public DateOnly? WorkDate { get; set; }

    [Required, StringLength(100)]
    public string ActionType { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Summary { get; set; }

    public byte Severity { get; set; }
    public int Priority { get; set; } = 100;
    public ActionItemStatus Status { get; set; } = ActionItemStatus.Open;
    public DateTime? DueAt { get; set; }

    [StringLength(500)]
    public string? DetailRoute { get; set; }

    [StringLength(100)]
    public string? ReferenceNo { get; set; }

    public string? PayloadJson { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DismissedAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
}

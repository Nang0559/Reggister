using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

[Table("F03CalendarProjection")]
public sealed class F03CalendarProjection : BaseAuditEntity
{
    public int EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }

    [Required, StringLength(50)]
    public string ModuleCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string SourceId { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string StatusCode { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Marker { get; set; }

    [StringLength(500)]
    public string? Summary { get; set; }

    public byte Severity { get; set; }
    public bool RequiresAction { get; set; }
    public Guid? ActionId { get; set; }

    [StringLength(500)]
    public string? DetailRoute { get; set; }

    [Required, StringLength(50)]
    public string SourceType { get; set; } = "MODULE";

    [StringLength(100)]
    public string? ParticipantId { get; set; }

    public string? PayloadJson { get; set; }
    public DateTime CalculatedAt { get; set; }
}

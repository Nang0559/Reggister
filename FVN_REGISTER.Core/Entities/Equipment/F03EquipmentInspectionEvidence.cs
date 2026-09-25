using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentInspectionEvidence")]
public sealed class F03EquipmentInspectionEvidence : BaseAuditEntity
{
    public int TaskId { get; set; }
    public int? ItemResultId { get; set; }
    [Required, StringLength(255)] public string FileName { get; set; } = string.Empty;
    [Required, StringLength(100)] public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    [Required, StringLength(500)] public string StoragePath { get; set; } = string.Empty;
    public F03EquipmentInspectionTask? Task { get; set; }
}
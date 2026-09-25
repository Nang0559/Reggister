using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentInspectionItemResults")]
public sealed class F03EquipmentInspectionItemResult : BaseAuditEntity
{
    public int TaskId { get; set; }
    public int ItemId { get; set; }
    [StringLength(2000)] public string? ValueText { get; set; }
    public decimal? ValueNumber { get; set; }
    public bool? Passed { get; set; }
    [StringLength(1000)] public string? Note { get; set; }
}
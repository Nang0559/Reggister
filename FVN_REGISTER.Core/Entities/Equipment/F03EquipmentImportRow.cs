using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentImportRows")]
public sealed class F03EquipmentImportRow : BaseAuditEntity
{
    public int BatchId { get; set; }
    public int RowNumber { get; set; }
    [Required] public string RawJson { get; set; } = "{}";
    [Required, StringLength(30)] public string Status { get; set; } = "Valid";
    [StringLength(2000)] public string? ErrorMessage { get; set; }
    public int? AssetId { get; set; }
}

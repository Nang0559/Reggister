using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentRepairHistory")]
public sealed class F03EquipmentRepairHistory : BaseAuditEntity
{
    public int AssetId { get; set; }
    public int RequestId { get; set; }
    public DateTime RepairDate { get; set; }
    public int OperatorUserId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? RepairCost { get; set; }
    [Required, StringLength(1000)] public string RepairContent { get; set; } = string.Empty;
    [StringLength(250)] public string? RepairVendor { get; set; }
    [StringLength(1000)] public string? RepairResult { get; set; }
    [StringLength(1000)] public string? Note { get; set; }
    public bool IsApproved { get; set; }
    public virtual F03EquipmentAsset Asset { get; set; } = null!;
}

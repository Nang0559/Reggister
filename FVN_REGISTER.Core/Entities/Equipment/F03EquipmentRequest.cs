using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Enums;
namespace FVN_REGISTER.Core.Entities.Equipment;
[Table("F03EquipmentRequests")]
public sealed class F03EquipmentRequest : BaseRequestEntity
{
    public EquipmentRequestKind RequestKind { get; set; }
    public int? AssetId { get; set; }
    [Required, StringLength(50)] public string SelectedApproverCode { get; set; } = string.Empty;
    [Required, StringLength(128)] public string QrToken { get; set; } = string.Empty;
    public int OperatorUserId { get; set; }
    [Required, StringLength(250)] public string EquipmentName { get; set; } = string.Empty;
    [StringLength(1000)] public string? Specification { get; set; }
    [StringLength(100)] public string? SerialNumber { get; set; }
    [StringLength(50)] public string? AssetCode { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PurchasePrice { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? ExpectedDepreciationDate { get; set; }
    [StringLength(250)] public string? Location { get; set; }
    [StringLength(1000)] public string? Note { get; set; }
    public DateTime? RepairDate { get; set; }
    [StringLength(1000)] public string? RepairContent { get; set; }
    [StringLength(250)] public string? RepairVendor { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? RepairCost { get; set; }
    [StringLength(1000)] public string? RepairResult { get; set; }
    public virtual F03EquipmentAsset? Asset { get; set; }
}

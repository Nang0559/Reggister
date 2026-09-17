using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Dtos.Equipment;

public sealed class CreateEquipmentRepairDto
{
    [Required] public int AssetId { get; set; }
    [Required] public DateTime RepairDate { get; set; }
    [Required, StringLength(1000)] public string RepairContent { get; set; } = string.Empty;
    [StringLength(250)] public string? RepairVendor { get; set; }
    [Range(0, 999999999999)] public decimal? RepairCost { get; set; }
    [StringLength(1000)] public string? RepairResult { get; set; }
    [StringLength(1000)] public string? Note { get; set; }
    [Required, StringLength(50)] public string SelectedApproverCode { get; set; } = string.Empty;
}

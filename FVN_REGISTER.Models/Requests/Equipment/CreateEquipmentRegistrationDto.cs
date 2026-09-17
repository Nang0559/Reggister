using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Dtos.Equipment;

public sealed class CreateEquipmentRegistrationDto
{
    [Required, StringLength(250)] public string EquipmentName { get; set; } = string.Empty;
    [StringLength(1000)] public string? Specification { get; set; }
    [StringLength(100)] public string? SerialNumber { get; set; }
    [StringLength(50)] public string? AssetCode { get; set; }
    [Range(0, 999999999999)] public decimal PurchasePrice { get; set; }
    [Required] public DateTime PurchaseDate { get; set; }
    [Required] public DateTime ExpectedDepreciationDate { get; set; }
    [Required, StringLength(20)] public string DeptCode { get; set; } = string.Empty;
    [StringLength(250)] public string? Location { get; set; }
    [Required, StringLength(50)] public string SelectedApproverCode { get; set; } = string.Empty;
    [StringLength(1000)] public string? Note { get; set; }
}

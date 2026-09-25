using FVN_REGISTER.Contract.Requests.Approvals;
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
    /// <summary>Optional initial asset custodian. Must resolve to an active HR employee when supplied.</summary>
    [StringLength(50)] public string? ResponsibleEmployeeCode { get; set; }
    [StringLength(50)] public string? SelectedApproverCode { get; set; }
    public List<ApprovalSelectionDto> ApprovalSelections { get; set; } = new();
    [StringLength(1000)] public string? Note { get; set; }
}

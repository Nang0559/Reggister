using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Equipment;

public sealed class EquipmentRequestDto
{
    public int Id { get; set; }
    public EquipmentRequestKind RequestKind { get; set; }
    public ApprovalStatus RequestStatus { get; set; }
    public int? AssetId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string? AssetCode { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? ExpectedDepreciationDate { get; set; }
    public DateTime? RepairDate { get; set; }
    public string? RepairContent { get; set; }
    public decimal? RepairCost { get; set; }
    public string QrToken { get; set; } = string.Empty;
    public string QrUrl { get; set; } = string.Empty;
    public string SelectedApproverCode { get; set; } = string.Empty;
}

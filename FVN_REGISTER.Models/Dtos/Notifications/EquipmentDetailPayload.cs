using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Notifications;

public sealed class EquipmentDetailPayload
{
    public string? EmployeeName { get; set; }
    public string? DeptName { get; set; }
    public EquipmentRequestKind RequestKind { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string? AssetCode { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? ExpectedDepreciationDate { get; set; }
    public DateTime? RepairDate { get; set; }
    public string? RepairContent { get; set; }
    public string? RepairVendor { get; set; }
    public decimal? RepairCost { get; set; }
    public string? RepairResult { get; set; }
    public string? Specification { get; set; }
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public string? Note { get; set; }
}
namespace FVN_REGISTER.Contract.Dtos.Equipment;

public sealed class EquipmentAssetDto
{
    public int Id { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public string? SerialNumber { get; set; }
    public string? AssetCode { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpectedDepreciationDate { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string QrToken { get; set; } = string.Empty;
    public string QrUrl { get; set; } = string.Empty;
    public bool IsQrActive { get; set; }
    public string? Note { get; set; }
    public List<EquipmentRepairHistoryDto> RepairHistory { get; set; } = new();
}

public sealed class EquipmentRepairHistoryDto
{
    public int Id { get; set; }
    public DateTime RepairDate { get; set; }
    public int OperatorUserId { get; set; }
    public string? OperatorName { get; set; }
    public decimal? RepairCost { get; set; }
    public string RepairContent { get; set; } = string.Empty;
    public string? RepairVendor { get; set; }
    public string? RepairResult { get; set; }
    public string? Note { get; set; }
}

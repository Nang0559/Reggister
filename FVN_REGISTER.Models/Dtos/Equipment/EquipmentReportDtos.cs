namespace FVN_REGISTER.Contract.Dtos.Equipment;

public sealed class EquipmentReportFilterDto
{
    public string? DeptCode { get; set; }
    public string? ResponsibleEmployeeCode { get; set; }
    public string? RepairResponsibleDeptCode { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public sealed class EquipmentReportRowDto
{
    public int AssetId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string? AssetCode { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string DeptCode { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal PurchasePrice { get; set; }
    public string? ResponsibleEmployeeCode { get; set; }
    public string? ResponsibleEmployeeName { get; set; }
    public string? OperatingResponsibleDeptCode { get; set; }
    public string? OperatingResponsibleEmployeeCode { get; set; }
    public string? OperatingResponsibleEmployeeName { get; set; }
    public DateTime? ResponsibleAssignedAt { get; set; }
    public int RepairCount { get; set; }
    public int HandoverCount { get; set; }
    public DateTime? LastRepairAt { get; set; }
    public DateTime? LastHandoverAt { get; set; }
    public int ChecklistCount { get; set; }
    public DateTime? LastChecklistAt { get; set; }
}

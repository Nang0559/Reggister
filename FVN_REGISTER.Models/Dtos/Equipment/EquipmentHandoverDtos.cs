namespace FVN_REGISTER.Contract.Dtos.Equipment;

public sealed class EquipmentHandoverRequest
{
    public string? OldResponsibleEmployeeCode { get; set; }
    public string NewResponsibleEmployeeCode { get; set; } = string.Empty;
    public string? OldApproverEmployeeCode { get; set; }
    public string? NewApproverEmployeeCode { get; set; }
    public string? DeptCode { get; set; }
    public List<int> AssetIds { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
}

public sealed class EquipmentHandoverEmployeeOptionDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DeptCode { get; set; } = string.Empty;
    public string PositionCode { get; set; } = string.Empty;
}

public sealed class EquipmentHandoverCandidateDto
{
    public int AssetId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string DeptCode { get; set; } = string.Empty;
    public string? ResponsibleEmployeeCode { get; set; }
    public string? ResponsibleEmployeeName { get; set; }
    public string? ApproverEmployeeCode { get; set; }
    public string? ApproverEmployeeName { get; set; }
    public string? Location { get; set; }
}

public sealed class EquipmentHandoverResultDto
{
    public int AssetCount { get; set; }
    public int InspectionAssignmentCount { get; set; }
    public int InspectionTaskCount { get; set; }
    public List<EquipmentHandoverCandidateDto> Assets { get; set; } = new();
}

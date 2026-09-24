using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Security;

public sealed class AccessChangeEmployeeOptionDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DeptCode { get; set; } = string.Empty;
    public string PositionCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class AccessChangeRequestCreateDto
{
    public RequestModule BusinessModule { get; set; }
    public string OldEmployeeCode { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public List<int> FunctionCodes { get; set; } = new();
    public List<int> EquipmentAssetIds { get; set; } = new();
    public bool TransferEquipmentResponsible { get; set; }
    public bool TransferEquipmentApprover { get; set; }
    public string? OldEquipmentApproverCode { get; set; }
}

public sealed class AccessChangeRequestDto
{
    public int Id { get; set; }
    public RequestModule BusinessModule { get; set; }
    public string BusinessModuleName { get; set; } = string.Empty;
    public string RequesterEmployeeCode { get; set; } = string.Empty;
    public string OldEmployeeCode { get; set; } = string.Empty;
    public string NewEmployeeCode { get; set; } = string.Empty;
    public string DeptCode { get; set; } = string.Empty;
    public string? NewPositionCode { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<int> FunctionCodes { get; set; } = new();
    public List<int> EquipmentAssetIds { get; set; } = new();
    public bool TransferEquipmentResponsible { get; set; }
    public bool TransferEquipmentApprover { get; set; }
    public AccessChangeStatus Status { get; set; }
    public string? PreChangeSnapshotJson { get; set; }
    public string? PostChangeResultJson { get; set; }
    public List<AccessChangeApprovalStepDto> ApprovalSteps { get; set; } = new();
    public string? ITNote { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ITCompletedAt { get; set; }
}

public sealed class AccessChangeApprovalStepDto
{
    public int Level { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string ApproverCode { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public string Decision { get; set; } = "Pending";
    public DateTime? ActionAt { get; set; }
}

public sealed class AccessChangeFunctionOptionDto
{
    public int FunctionCode { get; set; }
    public string FunctionName { get; set; } = string.Empty;
    public string? ModuleCode { get; set; }
    public string? ActionCode { get; set; }
    public string? ScopeCode { get; set; }
}

public sealed class AccessChangeItQueueItemDto
{
    public int Id { get; set; }
    public RequestModule BusinessModule { get; set; }
    public string BusinessModuleName { get; set; } = string.Empty;
    public string OldEmployeeCode { get; set; } = string.Empty;
    public string NewEmployeeCode { get; set; } = string.Empty;
    public string NewEmployeeName { get; set; } = string.Empty;
    public string DeptCode { get; set; } = string.Empty;
    public List<int> FunctionCodes { get; set; } = new();
    public bool TransferEquipmentResponsible { get; set; }
    public bool TransferEquipmentApprover { get; set; }
    public AccessChangeStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public sealed class AccessChangeItExecuteDto
{
    public string Note { get; set; } = string.Empty;
}

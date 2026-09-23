namespace FVN_REGISTER.Contract.Dtos.Security;

public sealed class ManagedScopeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public string? NodeCode { get; set; }
    public string? FactoryCode { get; set; }
    public string? DeptCode { get; set; }
    public string? SubDepartmentCode { get; set; }
    public bool IncludeChildren { get; set; }
    public string? Remark { get; set; }
}

public sealed class ManagedScopeRequest
{
    public string NodeType { get; set; } = "Department";
    public string? NodeCode { get; set; }
    public string? FactoryCode { get; set; }
    public string? DeptCode { get; set; }
    public string? SubDepartmentCode { get; set; }
    public bool IncludeChildren { get; set; } = true;
    public string? Remark { get; set; }
}

public sealed class EffectivePermissionPreviewDto
{
    public int UserId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string? DeptCode { get; set; }
    public string? PositionCode { get; set; }
    public List<int> RoleCodes { get; set; } = new();
    public List<string> RoleNames { get; set; } = new();
    public List<string> VisibleMenus { get; set; } = new();
    public List<EffectivePermissionActionDto> Actions { get; set; } = new();
    public List<ManagedScopeDto> ManagedScopes { get; set; } = new();
    public List<EffectiveApprovalPolicyDto> ApprovalPolicies { get; set; } = new();
    public List<string> BusinessStateConstraints { get; set; } = new();
}

public sealed class EffectivePermissionActionDto
{
    public int FunctionCode { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ActionCode { get; set; } = string.Empty;
    public string? ScopeCode { get; set; }
}

public sealed class EffectiveApprovalPolicyDto
{
    public int RequestType { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public string? DeptCode { get; set; }
    public string? PositionCode { get; set; }
    public string ApprovalPositionCode { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Sequence { get; set; }
    public string? LevelName { get; set; }
    public string? RoleName { get; set; }
}

namespace FVN_REGISTER.Contract.Dtos.Security;

public sealed class PermissionSnapshotDto
{
    public int UserId { get; set; }
    public List<int> RoleCodes { get; set; } = new();
    public List<SecurityFunctionDto> Functions { get; set; } = new();
    public HashSet<int> FunctionCodes { get; set; } = new();

    public bool Has(int functionCode) => FunctionCodes.Contains(functionCode);
}

public sealed class SecurityFunctionDto
{
    public int IdFunction { get; set; }
    public int FunctionCode { get; set; }
    public string FunctionName { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string? ModuleCode { get; set; }
    public string? ActionCode { get; set; }
    public string? ScopeCode { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class SecurityRoleDto
{
    public int IdRole { get; set; }
    public int RoleCode { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public List<int> FunctionCodes { get; set; } = new();
}

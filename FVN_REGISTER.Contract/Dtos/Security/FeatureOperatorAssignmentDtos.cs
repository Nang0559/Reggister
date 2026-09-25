namespace FVN_REGISTER.Contract.Dtos.Security;

public sealed class FeatureOperatorAssignmentDto
{
    public int Id { get; set; }
    public int FunctionCode { get; set; }
    public string FunctionName { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public int? ResourceId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? DeptCode { get; set; }
    public string? DeptName { get; set; }
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public string? Remark { get; set; }
}

public sealed class SaveFeatureOperatorAssignmentRequest
{
    public int FunctionCode { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public int? ResourceId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? Remark { get; set; }
}

public sealed class FeatureOperatorResourceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Status { get; set; }
}

namespace FVN_REGISTER.Contract.Dtos.Approvals;

public sealed class ApprovalPolicyDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public int RequestType { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;

    public string DeptCode { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;

    public string? PositionCode { get; set; }
    public string PositionName { get; set; } = string.Empty;

    public string ApprovalPositionCode { get; set; } = string.Empty;
    public string ApprovalPositionName { get; set; } = string.Empty;

    public int Level { get; set; }
    public int Sequence { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool Required { get; set; }
}

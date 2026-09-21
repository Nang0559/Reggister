namespace FVN_REGISTER.Contract.Dtos.Approvals;

public sealed class ApprovalPolicyPositionDto
{
    public string PositionCode { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public int? DefaultApproveLevel { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

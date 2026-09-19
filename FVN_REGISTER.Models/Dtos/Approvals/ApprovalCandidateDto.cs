namespace FVN_REGISTER.Contract.Dtos.Approvals;

public sealed class ApprovalCandidateDto
{
    public string ApproverCode { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public string? ApproverEmail { get; set; }
    public string ApproverDeptCode { get; set; } = string.Empty;
    public string ApproveForDeptCode { get; set; } = string.Empty;
}

namespace FVN_REGISTER.Contract.Dtos.Approvals;
public sealed class ApproverSyncProposalDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string OldDeptCode { get; set; } = string.Empty;
    public string OldPositionCode { get; set; } = string.Empty;
    public string NewDeptCode { get; set; } = string.Empty;
    public string NewPositionCode { get; set; } = string.Empty;
    public int? CurrentApproverId { get; set; }
    public string CurrentApproverCode { get; set; } = string.Empty;
    public int? CurrentLevel { get; set; }
    public string CurrentRoleName { get; set; } = string.Empty;
    public string CurrentApproveForDeptCode { get; set; } = string.Empty;
    public string? SuggestedApproverCode { get; set; }
    public int? SuggestedLevel { get; set; }
    public string? SuggestedRoleName { get; set; }
    public string? SuggestedApproveForDeptCode { get; set; }
    public DateTime DetectedAt { get; set; }
}
namespace FVN_REGISTER.Contract.Dtos.Approvals;

public sealed class ApprovalRouteLevelDto
{
    public int Level { get; set; }
    public int Sequence { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool Required { get; set; } = true;
    public List<ApprovalCandidateDto> Candidates { get; set; } = new();
    public string? SelectedApproverCode { get; set; }
}

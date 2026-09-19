namespace FVN_REGISTER.Contract.Requests.Approvals;

public sealed class ApprovalSelectionDto
{
    public int Level { get; set; }
    public string ApproverCode { get; set; } = string.Empty;
}

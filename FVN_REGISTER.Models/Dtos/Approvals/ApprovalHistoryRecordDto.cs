using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class ApprovalHistoryRecordDto
    {
        public int StepId { get; set; }
        public string ApproverCode { get; set; } = string.Empty;
        public string ApproverName { get; set; } = string.Empty;
        public DecisionType Decision { get; set; }
        public string? Comment { get; set; }
        public DateTime ActionAt { get; set; }
        public bool IsOverriddenByAdmin { get; set; }
        public string? OverriddenByName { get; set; }
        public DateTime? OverriddenAt { get; set; }
    }
}

using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Entities.Approvers
{
    public sealed class F03ApprovalHistory : BaseAuditEntity
    {
        public int Id { get; init; }
        public RequestModule RequestType { get; init; } // BẮT BUỘC ĐỂ PHÂN BIỆT LOẠI ĐƠN
        public int RequestId { get; init; }
        public int StepId { get; init; }
        public bool IsOverriddenByAdmin { get; set; } = false;
        public string ApproverCode { get; init; } = string.Empty;
        public string ApproverName { get; init; } = string.Empty;
        public string? OverriddenByCode { get; set; }    // Admin code — RIÊNG BIỆT
        public string? OverriddenByName { get; set; }
        public DateTime? OverriddenAt { get; set; }
        public DecisionType Decision { get; set; }
        public string? Comment { get; init; }
        public DateTime ActionAt { get; init; }
    }
}

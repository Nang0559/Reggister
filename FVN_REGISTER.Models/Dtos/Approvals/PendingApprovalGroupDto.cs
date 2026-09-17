

using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class PendingApprovalGroupDto
    {
        public RequestModule RequestType { get; set; }   // MỚI
        public int ApproverLevel { get; set; }
        public int Count { get; set; }
        public int OverriddenCount { get; set; }
        public List<PendingApprovalItemDto> Requests { get; set; } = new();
    }
}

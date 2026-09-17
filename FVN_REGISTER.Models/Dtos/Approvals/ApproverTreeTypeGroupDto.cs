


using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class ApproverTreeTypeGroupDto
    {
        public RequestModule RequestType { get; set; }
        public string RequestTypeDisplay { get; set; } = string.Empty;
        public List<ApproverDto> Approvers { get; set; } = new();
    }
}

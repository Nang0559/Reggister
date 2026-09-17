using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Dtos.OtReasons;
using FVN_REGISTER.Contract.Requests.OT;

namespace FVN_REGISTER.Contract.Dtos
{
    public class OTCombinedDataDto
    {
        public OTRequestUpsertDto OTForm { get; set; } = new();
        public List<OTEmployeeDto> DeptEmployees { get; set; } = new();
        public List<ApprovalStepSnapshotDto> ApprovalSteps { get; set; } = new();
        public OTBalanceDto? Balance { get; set; }
        public List<OTLimitRuleDto> LimitRules { get; set; } = new();
        public List<OTSummaryDto> RecentOTRequests { get; set; } = new();
        public List<OTSummaryDto> TodayOTRequests { get; set; } = new();
        public List<OTReasonCodeDto> OTReasonCategories { get; set; } = new();
    }
}

using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces;
using FVN_REGISTER.Contract.Requests;
using FVN_REGISTER.Core.Interfaces;



namespace FVN_REGISTER.Contract.Dtos.Leaves
{
    public class LeaveRequestDto : BaseRequestDto<LeaveRequestDetailDto>, IPendingRequestRow
    {
        public int WorkYear { get; set; }
        public DateTime StartDate => Details?.Any() == true ? Details.Min(x => x.LeaveDate) : DateTime.MinValue;
        public DateTime EndDate => Details?.Any() == true ? Details.Max(x => x.LeaveDate) : DateTime.MinValue;
        public decimal TotalLeaveDays => Details?.Where(x => x.IsCountedAsLeave).Sum(x => x.DayValue) ?? 0;
        public decimal TotalDay => Details?.Sum(x => x.DayValue) ?? 0;
        public string PrimaryLeaveType => Details?.FirstOrDefault()?.LeaveTypeName ?? "N/A";
        public int RequestId => Id;

    }
}

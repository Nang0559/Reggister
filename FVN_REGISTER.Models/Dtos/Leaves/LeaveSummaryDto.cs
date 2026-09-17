using FVN_REGISTER.Contract.Requests;

namespace FVN_REGISTER.Contract.Dtos.Leaves
{
    public class LeaveSummaryDto : BaseRequestSummaryDto
    {
        public override string Kind => "LEAVE";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
    }
}

using FVN_REGISTER.Contract.Requests;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTSummaryDto : BaseRequestSummaryDto
    {
        public override string Kind => "OT";
        public DateTime OTDate { get; set; }
        public decimal TotalHours { get; set; }
        public int EmployeeCount { get; set; }
    }
}

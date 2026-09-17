

using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Histories
{
    public class BalanceSummaryDto
    {
        public RequestModule Kind { get; set; } = RequestModule.Leave;
        // Leave
        public decimal? Entitled { get; set; }
        public decimal? Used { get; set; }
        public decimal? Remaining { get; set; }
        public decimal? Pending { get; set; }
        // OT
        public decimal? TotalOTHours { get; set; }
        public decimal? ApprovedOTHours { get; set; }
        public int? PendingCount { get; set; }
        public int? Year { get; set; }
    }
}

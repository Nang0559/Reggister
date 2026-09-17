using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Contract.Dtos
{
    public class RecentLeaveRequestDto
    {
        public int RequestId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApprovalStatus Status { get; set; } 
        public string? ApproverName { get; set; }          // Người duyệt (để biết đơn đang "kẹt" ở ai)
        
    }
}

using FVN_REGISTER.Contract.Dtos.Leaves;

namespace FVN_REGISTER.Contract.Dtos.Notifications
{
    public class LeaveDetailPayload
    {
        public string? EmployeeName { get; set; }
        public string? DeptName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDay { get; set; }
        public decimal TotalLeaveDay { get; set; }
        public string? LeaveReason { get; set; }
        public string? LeaveTypeName { get; set; }
        public List<LeaveRequestDetailDto> Details { get; set; } = new();
    }
}

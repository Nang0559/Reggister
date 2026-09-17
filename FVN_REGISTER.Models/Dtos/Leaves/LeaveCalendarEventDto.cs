using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Leaves
{
    /// <summary>
    /// Read-only calendar projection for the leave registration screen.
    /// UI must not depend on legacy ViewModels or persistence entities.
    /// </summary>
    public sealed class LeaveCalendarEventDto
    {
        public int RequestId { get; set; }
        public string LeaveCode { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime RegisterDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDay { get; set; }
        public decimal TotalLeaveDay { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = ApprovalStatus.Pending.ToString();
    }
}

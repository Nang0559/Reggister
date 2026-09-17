using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Entities.Views
{
    public partial class VF03LeaveRequestDetail
    {
        public long RowId { get; set; }

        public int LeaveId { get; set; }

        public string? EmployeeCode { get; set; }

        public string? EmployeeName { get; set; }

        public string? DeptCode { get; set; }

        public string? DeptName { get; set; }

        public ApprovalStatus RequestStatus { get; set; } 

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int DetailId { get; set; }

        public DateOnly LeaveDate { get; set; }

        public string LeaveTypeCode { get; set; } = null!;

        public string? LeaveTypeName { get; set; }

        public bool IsCountedAsLeave { get; set; }

        public bool IsHalfDay { get; set; }

        public string? HalfDayOption { get; set; }

        public decimal DayValue { get; set; }

        public DateTime? DetailCreatedAt { get; set; }

        public int? DetailCreatedBy { get; set; }
    }
}



namespace FVN_REGISTER.Contract.ViewModels
{
    public class LeaveBalanceViewModel
    {
        public string EmployeeCode { get; set; }

        public string EmployeeName { get; set; }

        public int WorkYear { get; set; }

        // Tổng phép được cấp trong năm
        public decimal TotalEntitledLeave { get; set; }

        // Đã sử dụng
        public decimal UsedLeave { get; set; }

        // Còn lại
        public decimal RemainingLeave { get; set; }

        // Nghỉ không lương (optional)
        public decimal UnpaidLeave { get; set; }

        // Nghỉ bệnh (optional)
        public decimal SickLeave { get; set; }
    }
}

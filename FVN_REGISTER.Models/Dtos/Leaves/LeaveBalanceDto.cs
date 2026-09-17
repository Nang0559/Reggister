namespace FVN_REGISTER.Contract.Dtos.Leaves
{
    public class LeaveBalanceDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int WorkYear { get; set; }

        public decimal TotalEntitled { get; set; } // Phép được cấp
    
        public decimal Used { get; set; }          // Đã dùng
        public decimal Remaining { get; set; }     // Còn lại
        public decimal Unpaid { get; set; }        // Nghỉ không lương
        public decimal Sick { get; set; }          // Nghỉ bệnh
        public int Year { get; set; }
    }
}

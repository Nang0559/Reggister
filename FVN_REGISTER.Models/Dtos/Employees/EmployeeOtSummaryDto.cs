

namespace FVN_REGISTER.Contract.Dtos.Employees
{
    /// <summary>
    /// Thống kê OT theo năm của một nhân viên
    /// </summary>
    public class EmployeeOtSummaryDto
    {
        public string EmployeeCode { get; set; } = "";
        public int Year { get; set; }
        public decimal TotalHours { get; set; }
        public int TotalDays { get; set; }
        public List<OtMonthlyDto> Monthly { get; set; } = new();
    }
}

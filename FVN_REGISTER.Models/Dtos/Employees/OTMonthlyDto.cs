

namespace FVN_REGISTER.Contract.Dtos.Employees
{
    /// <summary>
    /// Thống kê OT theo tháng — thuần số liệu, không có UI logic (MonthName đã bỏ)
    /// </summary>
    public class OtMonthlyDto
    {
        public int Month { get; set; }
        public decimal Hours { get; set; }
        public int Days { get; set; }
    }
}

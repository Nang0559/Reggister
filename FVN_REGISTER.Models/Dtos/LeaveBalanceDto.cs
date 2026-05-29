

namespace FVN_REGISTER.Contract.Dtos
{
    public class LeaveBalanceDto
    {
        public decimal Entitled { get; set; }   // Tổng phép năm được cấp (TongPhep)
        public decimal Used { get; set; }       // Đã sử dụng
        public decimal Remaining { get; set; }  // Còn lại (PhepTon)
        public decimal Pending { get; set; }    // Đang chờ duyệt
        public int Year { get; set; }
    }
}

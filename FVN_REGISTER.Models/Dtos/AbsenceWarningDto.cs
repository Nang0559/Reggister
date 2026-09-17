
namespace FVN_REGISTER.Contract.Dtos
{
    public class AbsenceWarningDto
    {
        public string DeptCode { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;

        public int TotalStaff { get; set; }        // Tổng nhân sự phòng ban
        public int CurrentAbsent { get; set; }     // Số người đã nghỉ/vắng hôm nay
        public int PendingRequests { get; set; }   // Số người đang chờ duyệt nghỉ hôm nay/mai

        public double AbsenceRate { get; set; }    // Tỷ lệ vắng mặt (%)
        public double Threshold { get; set; }      // Ngưỡng cảnh báo (ví dụ 10.0 hoặc 15.0)

        public bool IsCritical => AbsenceRate >= Threshold; // Vượt ngưỡng
        public string WarningMessage { get; set; } = string.Empty;
    }
}

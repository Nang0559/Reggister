

using FVN_REGISTER.Contract.Dtos;

namespace FVN_REGISTER.Contract.ViewModels
{
    public class DashboardViewModel
    {
        public CurrentUser? CurrentUser { get; set; }
        // Thay vì tách lẻ, hãy dùng các List DTO bạn đã định nghĩa
        public List<WidgetCounterDto> Widgets { get; set; } = new();
        public List<RecentLeaveRequestDto> RecentRequests { get; set; } = new();
        public LeaveBalanceDto? PersonalBalance { get; set; }
        public AbsenceWarningDto? DeptWarning { get; set; }

        // Thống kê biểu đồ vẫn giữ ViewModel nếu nó phức tạp
        public List<LeaveStatisticsViewModel> LeaveStatistics { get; set; } = new();
        public bool IsEPL { get; set; }
    }
}

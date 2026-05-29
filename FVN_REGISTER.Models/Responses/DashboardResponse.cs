using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Responses
{
    public class DashboardResponse
    {
        // Dành cho cấp Quản lý/HR
        public List<WidgetCounterDto> SummaryWidgets { get; set; } = new();
        public List<PendingRequestDto> PendingApprovals { get; set; } = new();
        public List<LeaveStatisticsViewModel> DeptStats { get; set; } = new();

        // Dành cho cá nhân nhân viên
        public LeaveBalanceDto? MyBalance { get; set; }
        public List<RecentLeaveRequestDto> MyRecentRequests { get; set; } = new();

        // Flag để UI biết nên render giao diện nào
        public bool ShowManagerView { get; set; }
    }
}

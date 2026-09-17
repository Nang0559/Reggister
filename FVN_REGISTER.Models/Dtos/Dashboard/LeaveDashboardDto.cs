using FVN_REGISTER.Contract.Dtos.Leaves;


namespace FVN_REGISTER.Contract.Dtos.Dashboard
{
    public class LeaveDashboardDto
    {
        // Thông tin cơ bản
        public List<WidgetCounterDto> Widgets { get; set; } = new();

        // Dữ liệu cho nhân viên (Personal View)
        public LeaveBalanceDto? PersonalBalance { get; set; }
        public List<LeaveSummaryDto> RecentRequests { get; set; } = new();

        // Dữ liệu cho quản lý (Management View) - Tách riêng để không load thừa
        public AbsenceWarningDto? DeptWarning { get; set; }
        public List<LeaveStatisticsDto> DepartmentStatistics { get; set; } = new();

        // Config/Flags
        public bool IsEPL { get; set; }
    }
}

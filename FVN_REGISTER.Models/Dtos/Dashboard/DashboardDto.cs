using FVN_REGISTER.Contract.Dtos.Actions;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.OT;


namespace FVN_REGISTER.Contract.Dtos.Dashboard
{
    public class DashboardDto
    {
        // Widget tổng hợp — mỗi module tự đóng góp, DashboardService gộp lại
        public List<WidgetCounterDto> Widgets { get; set; } = new();

        // Chờ duyệt — gộp mọi module, lấy thẳng từ IApprovalInboxService, KHÔNG tự query lại
        public List<PendingApprovalGroupDto> PendingApprovals { get; set; } = new();

        // Dữ liệu cá nhân theo từng module — null nếu module không áp dụng / user không có quyền
        public LeaveDashboardSectionDto? Leave { get; set; }
        public OTDashboardSectionDto? Overtime { get; set; }
        // Trip: chưa có nghiệp vụ hoàn chỉnh, để trống, bổ sung TripDashboardSectionDto sau

        // Dữ liệu quản lý (chỉ có nếu user có quyền quản lý phòng ban)
        public AbsenceWarningDto? DeptWarning { get; set; }
        public List<LeaveStatisticsDto> DepartmentStatistics { get; set; } = new();

        public bool ShowManagerView { get; set; }
        public bool IsEPL { get; set; }

        // Shared Action inbox: visible immediately on Dashboard/Login.
        public ActionCountDto ActionCount { get; set; } = new();
        public List<ActionItemDto> Actions { get; set; } = new();
    }
    public class LeaveDashboardSectionDto
    {
        public LeaveBalanceDto? Balance { get; set; }
        public List<LeaveSummaryDto> RecentRequests { get; set; } = new();
    }

    public class OTDashboardSectionDto
    {
        public OTBalanceDto? Balance { get; set; }
        public List<OTSummaryDto> RecentRequests { get; set; } = new();
    }
}

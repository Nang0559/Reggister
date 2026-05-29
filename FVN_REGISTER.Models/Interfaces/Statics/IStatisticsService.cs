using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Interfaces.Statics
{
    public interface IStatisticsService
    {
        // Thêm CancellationToken ct = default vào cuối mỗi hàm
        Task<List<LeaveStatisticsViewModel>> GetLeaveStatisticsAsync(bool includeCompanyTotal = false, CancellationToken ct = default);

        Task<int> GetTotalEmployeesAsync(CancellationToken ct = default);

        Task<int> GetPresentTodayAsync(CancellationToken ct = default);

        Task<int> GetHolidayCountAsync(int year, CancellationToken ct = default);

        Task<WorkYearStatsViewModel> GetWorkYearStatsAsync(CancellationToken ct = default);

        Task<LeaveStatisticsViewModel> GetDepartmentStatisticsAsync(string deptCode, CancellationToken ct = default);

        Task<List<LeaveStatisticsViewModel>> GetStatisticsByTimeRangeAsync(string[]? departments, TimeRange timeRange, CancellationToken ct = default);
        // nhân sự cảnh báo 
        Task<AbsenceWarningDto> GetAbsenceWarningAsync(string deptCode, CancellationToken ct = default);
        // Widget cho HR Dashboard: Tổng nhân viên, Tổng đơn chưa duyệt toàn công ty...
        Task<List<WidgetCounterDto>> GetCompanyDashboardWidgetsAsync(CancellationToken ct = default);
    }
}

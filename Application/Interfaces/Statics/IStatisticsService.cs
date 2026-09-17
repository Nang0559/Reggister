


using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Utils;


    namespace FVN_REGISTER.Application.Interfaces.Statics
    {
        public interface IStatisticsService
        {
            Task<List<LeaveStatisticsDto>> GetLeaveStatisticsAsync(bool includeCompanyTotal = false, CancellationToken ct = default);
            Task<int> GetTotalEmployeesAsync(CancellationToken ct = default);
            Task<int> GetPresentTodayAsync(CancellationToken ct = default);
            Task<int> GetHolidayCountAsync(int year, CancellationToken ct = default);
            Task<List<WorkYearDto>> GetWorkYearStatsAsync(CancellationToken ct = default);   // ← đổi: List thay vì object đơn
            Task<LeaveStatisticsDto> GetDepartmentStatisticsAsync(string deptCode, CancellationToken ct = default);
            Task<List<LeaveStatisticsDto>> GetStatisticsByTimeRangeAsync(string[]? departments, TimeRange timeRange, CancellationToken ct = default);
            Task<AbsenceWarningDto> GetAbsenceWarningAsync(string deptCode, CancellationToken ct = default);
            Task<List<WidgetCounterDto>> GetCompanyDashboardWidgetsAsync(CancellationToken ct = default);
            Task<List<WidgetCounterDto>> GetDeptDashboardWidgetsAsync(string deptCode, CancellationToken ct = default);
        }
    }


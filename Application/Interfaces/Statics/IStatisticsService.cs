using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Statics
{
    public interface IStatisticsService
    {
        Task<LeaveStatisticsDto> GetDepartmentStatisticsAsync(string deptCode, CancellationToken ct = default);
        Task<List<LeaveStatisticsDto>> GetStatisticsByTimeRangeAsync(string[]? departments, TimeRange timeRange, CancellationToken ct = default);
        Task<AbsenceWarningDto> GetAbsenceWarningAsync(string deptCode, CancellationToken ct = default);
        Task<List<WidgetCounterDto>> GetCompanyDashboardWidgetsAsync(CancellationToken ct = default);
        Task<List<WidgetCounterDto>> GetDeptDashboardWidgetsAsync(string deptCode, CancellationToken ct = default);
        Task<List<LeaveStatisticsDto>> GetLeaveStatisticsAsync(
            bool includeCompanyTotal = false,
            CancellationToken ct = default);
    }
}
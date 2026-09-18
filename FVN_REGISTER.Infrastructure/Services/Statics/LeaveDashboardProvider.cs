using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Services.Statics
{
    /// <summary>
    /// Leave-specific dashboard contribution. All Leave data access stays behind
    /// existing query/statistics ports; the provider does not introduce new EF queries.
    /// </summary>
    public sealed class LeaveDashboardProvider : IModuleDashboardProvider
    {
        private readonly ILeaveQueryService _leaveQuery;
        private readonly IStatisticsService _statistics;

        public RequestModule Module => RequestModule.Leave;

        public LeaveDashboardProvider(
            ILeaveQueryService leaveQuery,
            IStatisticsService statistics)
        {
            _leaveQuery = leaveQuery;
            _statistics = statistics;
        }

        public async Task<ModuleDashboardContribution> GetContributionAsync(
            UserIdentityDto user, CancellationToken ct = default)
        {
            var year = DateTime.Now.Year;

            var widgets = new List<WidgetCounterDto>(
                await _leaveQuery.GetMyWidgetsAsync(user.EmployeeCode!, ct));

            var balanceTask = _leaveQuery.GetSimpleBalanceAsync(
                user.EmployeeCode!, year, ct);
            var recentTask = _leaveQuery.GetRecentSummaryAsync(
                user.EmployeeCode!, 5, ct);

            AbsenceWarningDto? deptWarning = null;
            var departmentStatistics = new List<LeaveStatisticsDto>();

            if (user.Permission.IsApprover())
            {
                if (!string.IsNullOrWhiteSpace(user.DeptCode))
                {
                    deptWarning = await _statistics.GetAbsenceWarningAsync(
                        user.DeptCode, ct);
                }

                if (user.Permission.IsAdmin())
                {
                    departmentStatistics = await _statistics.GetLeaveStatisticsAsync(
                        includeCompanyTotal: true, ct);
                }
                else if (!string.IsNullOrWhiteSpace(user.DeptCode))
                {
                    departmentStatistics.Add(
                        await _statistics.GetDepartmentStatisticsAsync(
                            user.DeptCode, ct));
                }
            }

            await Task.WhenAll(balanceTask, recentTask);

            var detail = new LeaveDashboardDto
            {
                Widgets = widgets,
                PersonalBalance = await balanceTask,
                RecentRequests = await recentTask,
                DeptWarning = deptWarning,
                DepartmentStatistics = departmentStatistics
            };

            return new ModuleDashboardContribution
            {
                Module = Module,
                Widgets = widgets,
                Detail = detail
            };
        }
    }
}

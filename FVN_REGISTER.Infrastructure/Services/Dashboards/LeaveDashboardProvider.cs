using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Application.Interfaces.Dashboards;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Services.Dashboards
{
    /// <summary>
    /// Leave-specific dashboard contribution. All Leave data access stays behind
    /// existing query/statistics ports; the provider does not introduce new EF queries.
    /// </summary>
    public sealed class LeaveDashboardProvider : IModuleDashboardProvider
    {
        private readonly ILeaveQueryService _leaveQuery;
        private readonly IStatisticsService _statistics;
        private readonly IAuthorizationService _authorization;

        public RequestModule Module => RequestModule.Leave;
        public int RequiredFunctionCode => SecurityFunctionCodes.LeaveView;

        public LeaveDashboardProvider(
            ILeaveQueryService leaveQuery,
            IStatisticsService statistics,
            IAuthorizationService authorization)
        {
            _leaveQuery = leaveQuery;
            _statistics = statistics;
            _authorization = authorization;
        }

        public async Task<ModuleDashboardContribution> GetContributionAsync(
            UserIdentityDto user, CancellationToken ct = default)
        {
            var year = DateTime.Now.Year;

            var widgets = new List<WidgetCounterDto>(
                await _leaveQuery.GetMyWidgetsAsync(user.EmployeeCode!, ct));

            var personalBalance = await _leaveQuery.GetSimpleBalanceAsync(
                user.EmployeeCode!, year, ct);
            var recentSummary = await _leaveQuery.GetRecentSummaryAsync(
                user.EmployeeCode!, 5, ct);

            AbsenceWarningDto? deptWarning = null;
            var departmentStatistics = new List<LeaveStatisticsDto>();

            var canManageDepartment = !string.IsNullOrWhiteSpace(user.DeptCode)
                && await _authorization.CanAccessAsync(
                    user, SecurityFunctionCodes.LeaveView, null, user.DeptCode, ct);

            if (canManageDepartment)
            {
                deptWarning = await _statistics.GetAbsenceWarningAsync(
                    user.DeptCode!, ct);

                var scope = await _authorization.GetScopeAsync(
                    user.UserId, SecurityFunctionCodes.LeaveView, ct);

                if (string.Equals(scope, AuthorizationScopeCodes.All, StringComparison.OrdinalIgnoreCase))
                {
                    departmentStatistics = await _statistics.GetLeaveStatisticsAsync(
                        includeCompanyTotal: true, ct);
                }
                else
                {
                    departmentStatistics.Add(
                        await _statistics.GetDepartmentStatisticsAsync(
                            user.DeptCode!, ct));
                }
            }

            var detail = new LeaveDashboardDto
            {
                Widgets = widgets,
                PersonalBalance = personalBalance,
                RecentRequests = recentSummary,
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

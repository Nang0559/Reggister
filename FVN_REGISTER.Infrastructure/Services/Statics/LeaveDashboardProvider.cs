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
    /// KHÔNG tự viết query mới ở đây - chỉ tái sử dụng ILeaveQueryService (widget cá nhân,
    /// balance, recent requests) và IStatisticsService (widget phòng ban/công ty đã tách
    /// theo quyền ở lần review trước).
    /// </summary>
    public class LeaveDashboardProvider : IModuleDashboardProvider
    {
        private readonly ILeaveQueryService _leaveQuery;
        private readonly IStatisticsService _statistics;

        public RequestModule Module => RequestModule.Leave;

        public LeaveDashboardProvider(ILeaveQueryService leaveQuery, IStatisticsService statistics)
        {
            _leaveQuery = leaveQuery;
            _statistics = statistics;
        }

        public async Task<ModuleDashboardContribution> GetContributionAsync(UserIdentityDto user, CancellationToken ct = default)
        {
            var widgets = new List<WidgetCounterDto>(await _leaveQuery.GetMyWidgetsAsync(user.EmployeeCode!, ct));
            var year = DateTime.Now.Year;

            var balanceTask = _leaveQuery.GetSimpleBalanceAsync(user.EmployeeCode!, year, ct);
            var recentTask = _leaveQuery.GetRecentSummaryAsync(user.EmployeeCode!, 5, ct);   // SỬA: đúng tên method thật

            AbsenceWarningDto? deptWarning = null;
            List<LeaveStatisticsDto> deptStats = new();

            if (!string.IsNullOrEmpty(user.DeptCode) && user.Permission.IsApprover())
            {
                var deptWidgets = await _statistics.GetDeptDashboardWidgetsAsync(user.DeptCode, ct);
                foreach (var w in deptWidgets)
                    w.IsPersonal = false;   // widget quản lý, không phải "của tôi"

                widgets.AddRange(deptWidgets);
            }

            if (user.Permission.IsAdmin())
            {
                var companyWidgets = await _statistics.GetCompanyDashboardWidgetsAsync(ct);
                foreach (var w in companyWidgets)
                    w.IsPersonal = false;

                widgets.AddRange(companyWidgets);
            }

            await Task.WhenAll(balanceTask, recentTask);

            var detail = new LeaveDashboardDto
            {
                Widgets = widgets,
                PersonalBalance = await balanceTask,
                RecentRequests = await recentTask,   // SỬA: gán thẳng, kiểu đã khớp List<LeaveSummaryDto>, không cần map
                DeptWarning = deptWarning,
                DepartmentStatistics = deptStats
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

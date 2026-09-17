using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Infrastructure.Services.Statics
{ 
    public class OTDashboardProvider : IModuleDashboardProvider
    {
        private readonly IOTQueryService _otQuery;

        public RequestModule Module => RequestModule.Overtime;

        public OTDashboardProvider(IOTQueryService otQuery)
        {
            _otQuery = otQuery;
        }

        public async Task<ModuleDashboardContribution> GetContributionAsync(
     UserIdentityDto user, CancellationToken ct = default)
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;

            var widgets = new List<WidgetCounterDto>(
                await _otQuery.GetMyWidgetsAsync(user.EmployeeCode!, ct));

            var balanceTask = _otQuery.GetSimpleBalanceAsync(user.EmployeeCode!, year, ct);
            var recentTask = _otQuery.GetRecentSummaryAsync(user.EmployeeCode!, 5, ct);

            List<OTBalanceDto> nearLimitEmployees = new();
            if (!string.IsNullOrEmpty(user.DeptCode) && user.Permission.IsApprover())
            {
                nearLimitEmployees = await _otQuery.GetDeptNearLimitAsync(user.DeptCode, year, month, ct);

                // MỚI: thêm widget đếm số để hiện ngay trên dashboard tổng quan,
                // đối xứng với cách LeaveDashboardProvider thêm dept-widgets vào Widgets list.
                // Không tự tạo cảnh báo nếu không có ai gần vượt — tránh nhiễu Dashboard.
                if (nearLimitEmployees.Count > 0)
                {
                    widgets.Add(new WidgetCounterDto
                    {
                        Title = "NV gần vượt giới hạn OT",
                        Value = nearLimitEmployees.Count.ToString(),
                        Icon = "Warning",
                        Color = "Warning",
                        Link = "/ot/dept-status",
                        IsPersonal = false   // widget quản lý, không phải "của tôi"
                    });
                }
            }

            await Task.WhenAll(balanceTask, recentTask);

            var detail = new OTDashboardDto
            {
                Widgets = widgets,
                PersonalBalance = await balanceTask,
                RecentRequests = await recentTask,
                NearLimitEmployees = nearLimitEmployees
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

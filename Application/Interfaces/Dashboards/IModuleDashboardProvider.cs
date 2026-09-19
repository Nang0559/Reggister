using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Dashboards
{
    public interface IModuleDashboardProvider
    {
        RequestModule Module { get; }

        /// <summary>
        /// Quyền chức năng tối thiểu để provider được đưa vào Dashboard.
        /// Provider có thể override bằng capability của module (LeaveView, OTView, ...).
        /// </summary>
        string RequiredFunctionCode => SecurityFunctionCodes.DashboardView;

        Task<ModuleDashboardContribution> GetContributionAsync(
            UserIdentityDto user,
            CancellationToken ct = default);
    }
}

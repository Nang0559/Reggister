using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Statics
{
    public interface IDashboardService
    {
        Task<ServiceResult<DashboardDto>> GetDashboardAsync(
            UserIdentityDto user, CancellationToken ct = default);
    }
}
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Application.Interfaces.Statics
{
    public interface IDashboardService
    {
        Task<ServiceResult<DashboardResponse>> GetDashboardAsync(
            UserIdentityDto user, CancellationToken ct = default);
    }
}
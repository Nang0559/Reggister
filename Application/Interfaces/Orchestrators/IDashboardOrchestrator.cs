using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;



namespace FVN_REGISTER.Application.Interfaces.Orchestrators
{
    public interface IDashboardOrchestrator
    {
        Task<ServiceResult<DashboardResponse>> BuildAsync(UserIdentityDto user, CancellationToken ct = default);
    }
}

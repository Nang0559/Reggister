using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Dashboards
{
    public interface IDashboardClientService
    {
        Task<ApiResponse<DashboardResponse>> GetDashboardDataAsync(CancellationToken ct = default);
    }
}

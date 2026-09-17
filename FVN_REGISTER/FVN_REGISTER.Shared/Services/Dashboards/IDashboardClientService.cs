using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Dashboards
{
    public interface IDashboardClientService
    {
        Task<ApiResponse<DashboardDto>> GetDashboardDataAsync(CancellationToken ct = default);
    }
}

using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Shared.Services.Dashboards
{
    public interface IDashboardClientService
    {
        Task<ApiResponse<DashboardViewModel>> GetDashboardDataAsync(CancellationToken ct);
    }
}

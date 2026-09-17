using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels.Leaves;


namespace FVN_REGISTER.Shared.Services.Dashboards
{
    public interface IDashboardClientService
    {
        Task<ApiResponse<LeaveDashboardViewModel>> GetDashboardDataAsync(CancellationToken ct);
    }
}

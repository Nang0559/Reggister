using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Interfaces.Statics
{
    public interface IDashboardService
    {
        Task <ServiceResult<DashboardViewModel>> GetDashboardAsync(CurrentUser user,CancellationToken ct);
    }
}

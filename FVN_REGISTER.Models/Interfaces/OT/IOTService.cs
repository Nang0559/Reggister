using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;


namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTService
    {
        Task<ServiceResult> CreateAsync(CreateOTRequestModel model, CurrentUser user, CancellationToken ct = default);

        Task<ServiceResult> ApproveAsync(List<int> ids, int level, CurrentUser user, string? comment, CancellationToken ct = default);

        Task<ServiceResult> RejectAsync(List<int> ids, int level, CurrentUser user, string comment, CancellationToken ct = default);

        Task<ServiceResult> CancelAsync(int id, string reason, CurrentUser user, CancellationToken ct = default);

        Task<ServiceResult> ConfirmActualHoursAsync(int id, DateTime actualFrom, DateTime actualTo, CurrentUser user, CancellationToken ct = default);

        Task<ServiceResult<OTDetailViewModel>> GetDetailsAsync(int id, CancellationToken ct = default);
    }
}

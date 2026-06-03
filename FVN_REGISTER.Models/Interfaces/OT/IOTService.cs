using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;


namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTService
    {
        // Commands
        Task<ServiceResult<int>> CreateOTAsync(
            CreateOTRequestModel model, CurrentUser user, CancellationToken ct = default);

        Task<ServiceResult> ApproveAsync(
            List<int> ids, int level, CurrentUser user, string? comment, CancellationToken ct = default);

        Task<ServiceResult> RejectAsync(
            List<int> ids, int level, CurrentUser user, string? comment, CancellationToken ct = default);

        Task<ServiceResult> CancelAsync(
            int id, string? reason, CurrentUser user, CancellationToken ct = default);

        Task<ServiceResult> ValidateAndArchiveAsync(
            int id, CurrentUser user, string? note, CancellationToken ct = default);

        // Queries
        Task<ServiceResult<OTRequestViewModel>> GetDetailsAsync(
            int id, CancellationToken ct = default);

        Task<ServiceResult<OTBalanceDto>> GetOTBalanceAsync(
            string employeeCode, int year, int month, CancellationToken ct = default);
    }
}

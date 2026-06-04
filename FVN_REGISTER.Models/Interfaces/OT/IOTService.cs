using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;


namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTService
    {
        // ── COMMANDS ──────────────────────────────────────────────────────────
        Task<ServiceResult<int>> CreateAsync(
            CreateOTRequestModel model,
            CurrentUser user,
            CancellationToken ct = default);

        Task<ServiceResult> ApproveAsync(
            List<int> otRequestIds,
            int level,
            CurrentUser user,
            string? comment,
            CancellationToken ct = default);

        Task<ServiceResult> RejectAsync(
            List<int> otRequestIds,
            int level,
            CurrentUser user,
            string comment,
            CancellationToken ct = default);

        Task<ServiceResult> CancelAsync(
            int otRequestId,
            string reason,
            CurrentUser user,
            CancellationToken ct = default);

        // ── QUERIES ───────────────────────────────────────────────────────────
        Task<ServiceResult<OTRequestViewModel>> GetDetailsAsync(
            int otRequestId,
            CancellationToken ct = default);

        Task<ServiceResult<OTBalanceDto>> GetBalanceAsync(
            string employeeCode,
            int year,
            int month,
            CancellationToken ct = default);

        Task<ServiceResult<OTValidationResultDto>> ValidateHoursAsync(
            CreateOTRequestModel model,
            CancellationToken ct = default);
    }
}

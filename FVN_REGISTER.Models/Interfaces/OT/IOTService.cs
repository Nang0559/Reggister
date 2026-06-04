using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;


namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTService
    {
        // ── COMMANDS ──────────────────────────────────────────
        /// <summary>Tạo đơn OT mới (bao gồm danh sách nhân viên).</summary>
        Task<ServiceResult<int>> CreateAsync(
            CreateOTRequestModel model,
            CurrentUser creator,
            CancellationToken ct = default);

        /// <summary>Duyệt một hoặc nhiều đơn OT ở cấp level (3|5|6|7).</summary>
        Task<ServiceResult> ApproveAsync(
            List<int> ids,
            int level,
            CurrentUser approver,
            string? comment,
            CancellationToken ct = default);

        /// <summary>Từ chối một hoặc nhiều đơn OT ở cấp level.</summary>
        Task<ServiceResult> RejectAsync(
            List<int> ids,
            int level,
            CurrentUser approver,
            string? comment,
            CancellationToken ct = default);

        /// <summary>Hủy đơn OT (chủ đơn hoặc Admin).</summary>
        Task<ServiceResult> CancelAsync(
            int id,
            string reason,
            CurrentUser actor,
            CancellationToken ct = default);

        // ── QUERIES ───────────────────────────────────────────
        /// <summary>Chi tiết đơn OT theo Id.</summary>
        Task<ServiceResult<OTRequestViewModel>> GetDetailsAsync(
            int id,
            CancellationToken ct = default);

        /// <summary>Số dư giờ OT của một nhân viên (daily / monthly / yearly).</summary>
        Task<ServiceResult<OTBalanceDto>> GetBalanceAsync(
            string employeeCode,
            int year,
            int month,
            CancellationToken ct = default);
    }
}

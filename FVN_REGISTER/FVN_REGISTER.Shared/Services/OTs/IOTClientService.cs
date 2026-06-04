using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Shared.Utils.Helpers;

namespace FVN_REGISTER.Shared.Services.OT
{
    public interface IOTClientService
    {
        // ===== COMMANDS =====

        /// <summary>Tạo đơn OT mới</summary>
        Task<ApiResponse<object>> CreateOTRequestAsync(
            CreateOTRequestModel model,
            CancellationToken ct = default);

        /// <summary>Phê duyệt danh sách đơn OT</summary>
        Task<ApiResponse<object>> ApproveAsync(
            OTApproveRequest request,
            CancellationToken ct = default);

        /// <summary>Từ chối danh sách đơn OT</summary>
        Task<ApiResponse<object>> RejectAsync(
            OTApproveRequest request,
            CancellationToken ct = default);

        /// <summary>Hủy đơn OT</summary>
        Task<ApiResponse<object>> CancelAsync(
            int otRequestId,
            string reason,
            CancellationToken ct = default);

        // ===== QUERIES =====

        /// <summary>Lấy dữ liệu tổng hợp cho trang tạo đơn OT</summary>
        Task<ApiResponse<CreateOTRequestModel>> GetCombinedDataAsync(
            CancellationToken ct = default);

        /// <summary>Lấy số dư giờ OT</summary>
        Task<ApiResponse<OTBalanceDto>> GetOTBalanceAsync(
            int year,
            CancellationToken ct = default);

        /// <summary>Lấy lịch sử đơn OT</summary>
        Task<ApiResponse<List<OTRequestViewModel>>> GetRecentOTRequestsAsync(
            int limit = 10,
            CancellationToken ct = default);

        /// <summary>Lấy chi tiết đơn OT</summary>
        Task<ApiResponse<OTRequestViewModel>> GetOTDetailAsync(
            int otRequestId,
            CancellationToken ct = default);

        /// <summary>Lấy danh sách chờ duyệt</summary>
        Task<ApiResponse<List<OTRequestViewModel>>> GetPendingApprovalsAsync(
            int level = 0,
            CancellationToken ct = default);

        /// <summary>Lấy danh sách OT có phân trang (HR/Admin)</summary>
        Task<ApiResponse<PaginationResult<OTRequestViewModel>>> GetPagedOTRequestsAsync(
            string? deptCode,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        /// <summary>Lấy dữ liệu dashboard OT</summary>
        Task<ApiResponse<OTDashboardViewModel>> GetOTDashboardAsync(
            CancellationToken ct = default);

        /// <summary>Validate giờ OT real-time</summary>
        Task<ApiResponse<OTValidationResultDto>> ValidateOTHoursAsync(
            string employeeCode,
            DateTime otDate,
            decimal hours,
            string otType,
            CancellationToken ct = default);
    }
}
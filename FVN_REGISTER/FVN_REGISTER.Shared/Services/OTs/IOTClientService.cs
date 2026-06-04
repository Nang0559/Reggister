using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Shared.Utils.Helpers;

namespace FVN_REGISTER.Shared.Services.OTs
{
    public interface IOTClientService
    {
        // ── Commands ──
        Task<ApiResponse<object>> CreateOTRequestAsync(CreateOTRequestModel model, CancellationToken ct = default);
        Task<ApiResponse<object>> ApproveAsync(OTApproveRequest request, CancellationToken ct = default);
        Task<ApiResponse<object>> RejectAsync(OTApproveRequest request, CancellationToken ct = default);
        Task<ApiResponse<object>> CancelAsync(int otRequestId, string? reason, CancellationToken ct = default);

        // ── Queries ──
        Task<ApiResponse<CombinedOTViewModel>> GetCombinedDataAsync(CancellationToken ct = default);
        Task<ApiResponse<OTBalanceDto>> GetOTBalanceAsync(int year, CancellationToken ct = default);
        Task<ApiResponse<OTRequestViewModel>> GetOTDetailAsync(int otRequestId, CancellationToken ct = default);
        Task<ApiResponse<List<OTRequestViewModel>>> GetRecentOTRequestsAsync(int limit = 10, CancellationToken ct = default);
        Task<ApiResponse<List<OTRequestViewModel>>> GetPendingApprovalsAsync(int level = 0, CancellationToken ct = default);
        Task<ApiResponse<PaginationResult<OTRequestViewModel>>> GetPagedOTRequestsAsync(string? deptCode, string? status, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20, CancellationToken ct = default);
        Task<ApiResponse<OTDashboardViewModel>> GetOTDashboardAsync(CancellationToken ct = default);
        Task<ApiResponse<OTValidationResultDto>> ValidateOTHoursAsync(string employeeCode, DateTime otDate, decimal hours, string otType, CancellationToken ct = default);
    }
}
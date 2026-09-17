using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Utils;
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
        Task<ApiResponse<List<OTRequestViewModel>>> GetDeptOTByDateAsync(
         DateTime date, CancellationToken ct = default);
        Task<ApiResponse<List<OTEmployeeModel>>> GetDeptEmployeesAsync(string? deptCode = null, CancellationToken ct = default);
        Task<ApiResponse<List<OTRequestViewModel>>> GetMyHistoryAsync(int? year = null, CancellationToken ct = default);
        Task<ApiResponse<OTBalanceDto>> GetEmployeeBalanceAsync(
         string employeeCode, int year, CancellationToken ct = default);
        Task<ApiResponse<OTCombinedDataDto>> GetCombinedDataAsync(CancellationToken ct = default);
        Task<ApiResponse<OTBalanceDto>> GetOTBalanceAsync(int year, CancellationToken ct = default);
       
        Task<ApiResponse<List<OTRequestViewModel>>> GetRecentOTRequestsAsync(int limit = 10, CancellationToken ct = default);
        Task<ApiResponse<List<OTRequestViewModel>>> GetPendingApprovalsAsync(int level = 0, CancellationToken ct = default);
        Task<ApiResponse<PaginationResult<OTRequestViewModel>>> GetPagedOTRequestsAsync(string? deptCode, string? status, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20, CancellationToken ct = default);
        Task<ApiResponse<OTDashboardViewModel>> GetOTDashboardAsync(CancellationToken ct = default);
        Task<ApiResponse<OTValidationResultDto>> ValidateOTHoursAsync(string employeeCode, DateTime otDate, decimal hours, string otType, CancellationToken ct = default);

        //-- Chức năng Add Detail---//
        // FVN_REGISTER.Contract/Interfaces/OT/IOTClientService.cs

        Task<ApiResponse<object>> UpdateEmployeeOTInfoAsync(
            int otRequestId,
            List<OTEmployeeModel> employees,
            CancellationToken ct = default);
        Task<ApiResponse<OTRequestViewModel>> GetDetailAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> JoinOTRequestAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> RemoveEmployeeAsync(
        int otRequestId,
        string employeeCode,
        CancellationToken ct = default);
    }
}
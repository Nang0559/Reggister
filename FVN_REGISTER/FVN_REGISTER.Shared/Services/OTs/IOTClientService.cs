using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Dtos.OTTypeDtos;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Shared.Services.OTs;

public interface IOTClientService
{
    Task<ApiResponse<object>> CreateOTRequestAsync(OTRequestUpsertDto request, CancellationToken ct = default);
    Task<ApiResponse<object>> ApproveAsync(ApprovalActionDto request, CancellationToken ct = default);
    Task<ApiResponse<object>> RejectAsync(ApprovalActionDto request, CancellationToken ct = default);
    Task<ApiResponse<object>> CancelAsync(int otRequestId, string? reason, CancellationToken ct = default);

    Task<ApiResponse<List<OTRequestDto>>> GetDeptOTByDateAsync(DateTime date, CancellationToken ct = default);
    Task<ApiResponse<List<OTEmployeeDto>>> GetDeptEmployeesAsync(string? deptCode = null, CancellationToken ct = default);
    Task<ApiResponse<List<OTSummaryDto>>> GetMyHistoryAsync(int? year = null, CancellationToken ct = default);
    Task<ApiResponse<OTBalanceDto>> GetEmployeeBalanceAsync(string employeeCode, int year, CancellationToken ct = default);
    Task<ApiResponse<OTCombinedDataDto>> GetCombinedDataAsync(CancellationToken ct = default);
    Task<ApiResponse<List<OTTypeDto>>> GetActiveOTTypesAsync(CancellationToken ct = default);
    Task<ApiResponse<OTBalanceDto>> GetOTBalanceAsync(int year, CancellationToken ct = default);
    Task<ApiResponse<List<OTSummaryDto>>> GetRecentOTRequestsAsync(int limit = 10, CancellationToken ct = default);
    Task<ApiResponse<List<PendingApprovalItemDto>>> GetPendingApprovalsAsync(int level = 0, CancellationToken ct = default);
    Task<ApiResponse<PaginationResult<OTSummaryDto>>> GetPagedOTRequestsAsync(string? deptCode, string? status, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ApiResponse<object>> GetOTDashboardAsync(CancellationToken ct = default);
    Task<ApiResponse<OTValidationResultDto>> ValidateOTHoursAsync(string employeeCode, DateTime otDate, decimal hours, string otType, CancellationToken ct = default);

    Task<ApiResponse<object>> UpdateEmployeeOTInfoAsync(int otRequestId, List<OTEmployeeDto> employees, CancellationToken ct = default);
    Task<ApiResponse<OTRequestDto>> GetDetailAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<object>> JoinOTRequestAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<object>> RemoveEmployeeAsync(int otRequestId, string employeeCode, CancellationToken ct = default);
}
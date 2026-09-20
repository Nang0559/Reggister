using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Dtos.OTTypeDtos;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.OTs;

public sealed class OTClientService : IOTClientService
{
    private readonly IHttpClientWithAuth _http;
    private const string Base = "api/OT";

    public OTClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<object>> CreateOTRequestAsync(OTRequestUpsertDto request, CancellationToken ct = default)
        => _http.PostAsync<object>($"{Base}/create", request, ct);

    public Task<ApiResponse<object>> ApproveAsync(ApprovalActionDto request, CancellationToken ct = default)
        => SendApprovalAsync($"{Base}/approve", request, ct);

    public Task<ApiResponse<object>> RejectAsync(ApprovalActionDto request, CancellationToken ct = default)
        => SendApprovalAsync($"{Base}/reject", request, ct);

    private Task<ApiResponse<object>> SendApprovalAsync(string url, ApprovalActionDto request, CancellationToken ct)
        => _http.PostAsync<object>(url, new { Ids = request.RequestIds, Level = request.Level, Comment = request.Comment }, ct);

    public Task<ApiResponse<object>> CancelAsync(int otRequestId, string? reason, CancellationToken ct = default)
        => _http.PostAsync<object>($"{Base}/cancel/{otRequestId}", new { Reason = reason }, ct);

    public Task<ApiResponse<List<OTRequestDto>>> GetDeptOTByDateAsync(DateTime date, CancellationToken ct = default)
        => _http.GetAsync<List<OTRequestDto>>($"{Base}/dept-ot-by-date?date={date:yyyy-MM-dd}", ct);

    public Task<ApiResponse<List<OTEmployeeDto>>> GetDeptEmployeesAsync(string? deptCode = null, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(deptCode) ? $"{Base}/dept-employees" : $"{Base}/dept-employees?deptCode={Uri.EscapeDataString(deptCode)}";
        return _http.GetAsync<List<OTEmployeeDto>>(url, ct);
    }

    public Task<ApiResponse<List<OTSummaryDto>>> GetMyHistoryAsync(int? year = null, CancellationToken ct = default)
        => _http.GetAsync<List<OTSummaryDto>>(year.HasValue ? $"{Base}/history?year={year.Value}" : $"{Base}/history", ct);

    public Task<ApiResponse<OTBalanceDto>> GetEmployeeBalanceAsync(string employeeCode, int year, CancellationToken ct = default)
        => _http.GetAsync<OTBalanceDto>($"{Base}/balance/{Uri.EscapeDataString(employeeCode)}/{year}/1", ct);

    public Task<ApiResponse<OTCombinedDataDto>> GetCombinedDataAsync(CancellationToken ct = default)
        => _http.GetAsync<OTCombinedDataDto>($"{Base}/combined-data", ct);

    public Task<ApiResponse<List<OTTypeDto>>> GetActiveOTTypesAsync(CancellationToken ct = default)
        => _http.GetAsync<List<OTTypeDto>>("api/OTType/active", ct);
    public Task<ApiResponse<OTBalanceDto>> GetOTBalanceAsync(int year, CancellationToken ct = default)
        => _http.GetAsync<OTBalanceDto>($"{Base}/balance/{year}", ct);

    public Task<ApiResponse<List<OTSummaryDto>>> GetRecentOTRequestsAsync(int limit = 10, CancellationToken ct = default)
        => _http.GetAsync<List<OTSummaryDto>>($"{Base}/recent?limit={limit}", ct);

    public Task<ApiResponse<List<PendingApprovalItemDto>>> GetPendingApprovalsAsync(int level = 0, CancellationToken ct = default)
        => _http.GetAsync<List<PendingApprovalItemDto>>($"{Base}/pending?level={level}", ct);

    public Task<ApiResponse<PaginationResult<OTSummaryDto>>> GetPagedOTRequestsAsync(string? deptCode, string? status, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var url = $"{Base}/list?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(deptCode)) url += $"&deptCode={Uri.EscapeDataString(deptCode)}";
        if (!string.IsNullOrWhiteSpace(status)) url += $"&status={Uri.EscapeDataString(status)}";
        if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
        if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";
        return _http.GetPagedAsync<OTSummaryDto>(url, ct);
    }

    public Task<ApiResponse<object>> GetOTDashboardAsync(CancellationToken ct = default)
        => _http.GetAsync<object>($"{Base}/dashboard", ct);

    public Task<ApiResponse<OTValidationResultDto>> ValidateOTHoursAsync(string employeeCode, DateTime otDate, decimal hours, string otType, CancellationToken ct = default)
        => _http.PostAsync<OTValidationResultDto>($"{Base}/validate", new { EmployeeCode = employeeCode, OTDate = otDate, Hours = hours, OTType = otType }, ct);

    public Task<ApiResponse<OTValidationResultDto>> ValidateOTRequestAsync(OTRequestUpsertDto request, CancellationToken ct = default)
        => _http.PostAsync<OTValidationResultDto>($"{Base}/validate-request", request, ct);

    public Task<ApiResponse<OTLimitPreviewDto>> GetOTLimitPreviewAsync(OTRequestUpsertDto request, CancellationToken ct = default)
        => _http.PostAsync<OTLimitPreviewDto>($"{Base}/limit-preview", request, ct);

    public Task<ApiResponse<object>> UpdateEmployeeOTInfoAsync(int otRequestId, List<OTEmployeeDto> employees, CancellationToken ct = default)
        => _http.PutAsync<object>($"{Base}/{otRequestId}/employees/update", employees, ct);

    public Task<ApiResponse<OTRequestDto>> GetDetailAsync(int id, CancellationToken ct = default)
        => _http.GetAsync<OTRequestDto>($"{Base}/detail/{id}", ct);

    public Task<ApiResponse<object>> JoinOTRequestAsync(int id, CancellationToken ct = default)
        => _http.PostAsync<object>($"{Base}/join/{id}", new { }, ct);

    public Task<ApiResponse<object>> RemoveEmployeeAsync(int otRequestId, string employeeCode, CancellationToken ct = default)
        => _http.PostAsync<object>($"{Base}/{otRequestId}/remove-employee/{Uri.EscapeDataString(employeeCode)}", new { }, ct);
}
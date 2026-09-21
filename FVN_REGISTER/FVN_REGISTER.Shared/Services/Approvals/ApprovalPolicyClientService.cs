using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Approvals;

public sealed class ApprovalPolicyClientService : IApprovalPolicyClientService
{
    private readonly IHttpClientWithAuth _http;
    private const string Base = "api/approval-policies";

    public ApprovalPolicyClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<List<ApprovalPolicyDto>>> GetAllAsync(CancellationToken ct = default)
        => _http.GetAsync<List<ApprovalPolicyDto>>(Base, ct);

    public Task<ApiResponse<List<ApprovalPolicyPositionDto>>> GetPositionsAsync(CancellationToken ct = default)
        => _http.GetAsync<List<ApprovalPolicyPositionDto>>($"{Base}/positions", ct);

    public Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync(CancellationToken ct = default)
        => _http.GetAsync<List<DepartmentDto>>($"{Base}/departments", ct);

    public Task<ApiResponse<ApprovalPolicyDto>> CreateAsync(ApprovalPolicyRequest request, CancellationToken ct = default)
        => _http.PostAsync<ApprovalPolicyDto>(Base, request, ct);

    public Task<ApiResponse<ApprovalPolicyDto>> UpdateAsync(int id, ApprovalPolicyRequest request, CancellationToken ct = default)
        => _http.PutAsync<ApprovalPolicyDto>($"{Base}/{id}", request, ct);

    public Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default)
        => _http.DeleteAsync<object>($"{Base}/{id}", ct);
}

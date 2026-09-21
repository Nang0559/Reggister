using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Approvals;

public interface IApprovalPolicyClientService
{
    Task<ApiResponse<List<ApprovalPolicyDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<List<ApprovalPolicyPositionDto>>> GetPositionsAsync(CancellationToken ct = default);
    Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync(CancellationToken ct = default);
    Task<ApiResponse<ApprovalPolicyDto>> CreateAsync(ApprovalPolicyRequest request, CancellationToken ct = default);
    Task<ApiResponse<ApprovalPolicyDto>> UpdateAsync(int id, ApprovalPolicyRequest request, CancellationToken ct = default);
    Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default);
}

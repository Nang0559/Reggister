using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Approvals;

public interface IApprovalPolicyService
{
    Task<ServiceResult<List<ApprovalPolicyDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<List<ApprovalPolicyPositionDto>>> GetPositionsAsync(CancellationToken ct = default);
    Task<ServiceResult<List<DepartmentDto>>> GetDepartmentsAsync(CancellationToken ct = default);

    Task<ServiceResult<ApprovalPolicyDto>> CreateAsync(
        ApprovalPolicyRequest request, int actorUserId, CancellationToken ct = default);

    Task<ServiceResult<ApprovalPolicyDto>> UpdateAsync(
        int id, ApprovalPolicyRequest request, int actorUserId, CancellationToken ct = default);

    Task<ServiceResult<object>> DeleteAsync(
        int id, int actorUserId, CancellationToken ct = default);

    Task<bool> CanApproveAsync(
        RequestModule requestType,
        string requesterEmployeeCode,
        string approverEmployeeCode,
        int level,
        CancellationToken ct = default);
}

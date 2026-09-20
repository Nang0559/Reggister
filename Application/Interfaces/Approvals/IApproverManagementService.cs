

using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Application.Interfaces.Approvals
{
    public interface IApproverManagementService
    {
        // ══════════════════════════════════════════════════════════════════
        // QUERIES
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Lấy cây Approver: Dept -> Level -> RequestType -> List Approver.
        /// </summary>
        Task<ServiceResult<List<ApproverTreeNodeDto>>> GetApproverTreeAsync(
            CancellationToken ct);

        Task<ServiceResult<List<ApproverDto>>> GetListAsync(
            string? deptCode,
            int? level,
            RequestModule? requestType,
            CancellationToken ct);

        Task<ServiceResult<List<DepartmentDto>>> GetDepartmentsAsync(
            CancellationToken ct);

        Task<ServiceResult<List<EmployeeSelectDto>>> GetEmployeesAsync(
            string? deptCode,
            CancellationToken ct);

        // ══════════════════════════════════════════════════════════════════
        // COMMANDS
        // ══════════════════════════════════════════════════════════════════

        Task<ServiceResult> CreateAsync(ApproverDto model, int currentUserId, CancellationToken ct);
        Task<ServiceResult> UpdateAsync(ApproverDto model, int currentUserId, CancellationToken ct);
        Task<ServiceResult> DeleteAsync(int id, int currentUserId, CancellationToken ct);
        Task<ServiceResult> ToggleActiveAsync(int id, int currentUserId, CancellationToken ct);
        Task<ServiceResult<List<ApproverSyncProposalDto>>> GetSyncProposalsAsync(CancellationToken ct);
        Task<ServiceResult> AcceptSyncProposalAsync(int flagId, int currentUserId, CancellationToken ct);
        Task<ServiceResult> KeepSyncProposalAsync(int flagId, int currentUserId, CancellationToken ct);

        // ══════════════════════════════════════════════════════════════════
        // HELPERS DÙNG CHO PROVIDER (BuildHierarchy)
        // ══════════════════════════════════════════════════════════════════
    }
}



using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Utils;

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

        // ══════════════════════════════════════════════════════════════════
        // HELPERS DÙNG CHO PROVIDER (BuildHierarchy)
        // ══════════════════════════════════════════════════════════════════

        Task<int> GetRequesterLevelAsync(string employeeCode, RequestModule requestType, CancellationToken ct);
        Task<F03Approver?> GetApproverAsync(int level, string approveForDeptCode, RequestModule requestType, CancellationToken ct);
        Task<List<F03Approver>> GetApproversForDeptAsync(string approveForDeptCode, RequestModule requestType, CancellationToken ct);
        Task<string> ResolveApproverEmailAsync(string employeeCode, RequestModule requestType, CancellationToken ct = default);
    }
}

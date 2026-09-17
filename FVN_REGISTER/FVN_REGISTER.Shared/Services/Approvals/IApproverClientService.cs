using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Approvals
{
    public interface IApproverClientService
    {
        Task<ApiResponse<List<ApproverTreeNodeDto>>> GetTreeAsync(CancellationToken ct = default);
        Task<ApiResponse<List<ApproverDto>>> GetListAsync(
            string? deptCode,
            int? level,
            string? requestType,
            CancellationToken ct = default);
        Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync(CancellationToken ct = default);
        Task<ApiResponse<List<EmployeeSelectDto>>> GetEmployeesAsync(
            string? deptCode,
            CancellationToken ct = default);
        Task<ApiResponse<object>> CreateAsync(ApproverDto model, CancellationToken ct = default);
        Task<ApiResponse<object>> UpdateAsync(int id, ApproverDto model, CancellationToken ct = default);
        Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> ToggleAsync(int id, CancellationToken ct = default);
    }
}



using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Approvals
{
    public interface IApproverClientService
    {
        Task<ApiResponse<List<ApproverTreeNodeViewModel>>> GetTreeAsync(CancellationToken ct = default);
        Task<ApiResponse<List<ApproverViewModel>>> GetListAsync(
            string? deptCode,
            int? level,
            string? requestType,
            CancellationToken ct = default);
        Task<ApiResponse<List<DepartmentViewModel>>> GetDepartmentsAsync(CancellationToken ct = default);
        Task<ApiResponse<List<EmployeeSelectViewModel>>> GetEmployeesAsync(string? deptCode, CancellationToken ct = default);
        Task<ApiResponse<object>> CreateAsync(ApproverViewModel model, CancellationToken ct = default);
        Task<ApiResponse<object>> UpdateAsync(int id, ApproverViewModel model, CancellationToken ct = default);
        Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> ToggleAsync(int id, CancellationToken ct = default);
    }
}




using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Employees
{
    public interface IEmployeeManagementClientService
    {
        Task<ApiResponse<List<EmployeeDeptTreeViewModel>>> GetTreeAsync(
         string? searchTerm = null,
         string? deptCode = null,
         CancellationToken ct = default);

        Task<ApiResponse<EmployeeCardViewModel>> GetByIdAsync(
            int id, CancellationToken ct = default);

        Task<ApiResponse<List<DepartmentViewModel>>> GetCvListAsync(
            CancellationToken ct = default);

        Task<ApiResponse<EmployeeOtSummaryViewModel>> GetOtSummaryAsync(
            string employeeCode,
            int? year = null,
            CancellationToken ct = default);

        Task<ApiResponse<object>> CreateAsync(
            EmployeeFormViewModel model, CancellationToken ct = default);

        Task<ApiResponse<object>> UpdateAsync(
            int id, EmployeeFormViewModel model, CancellationToken ct = default);

        Task<ApiResponse<object>> ToggleAsync(
            int id, CancellationToken ct = default);

        Task<ApiResponse<object>> DeleteAsync(
            int id, CancellationToken ct = default);
    }

}

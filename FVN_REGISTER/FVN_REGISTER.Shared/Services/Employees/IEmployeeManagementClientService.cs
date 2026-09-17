using FVN_REGISTER.Contract.Dtos.Employees;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Employees
{
    public interface IEmployeeManagementClientService
    {
        Task<ApiResponse<List<EmployeeDeptTreeDto>>> GetTreeAsync(
            string? searchTerm = null,
            string? deptCode = null,
            CancellationToken ct = default);

        Task<ApiResponse<EmployeeCardDto>> GetByIdAsync(
            int id,
            CancellationToken ct = default);

        Task<ApiResponse<EmployeeOtSummaryDto>> GetOtSummaryAsync(
            string employeeCode,
            int? year = null,
            CancellationToken ct = default);

        Task<ApiResponse<object>> CreateAsync(
            EmployeeUpsertDto model,
            CancellationToken ct = default);

        Task<ApiResponse<object>> UpdateAsync(
            int id,
            EmployeeUpsertDto model,
            CancellationToken ct = default);

        Task<ApiResponse<object>> ToggleAsync(
            int id,
            CancellationToken ct = default);

        Task<ApiResponse<object>> DeleteAsync(
            int id,
            CancellationToken ct = default);
    }
}

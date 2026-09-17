using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Departments
{
    public interface IDepartmentManagementClientService
    {
        Task<ApiResponse<List<DepartmentDto>>> GetTreeAsync(CancellationToken ct = default);

        Task<ApiResponse<List<DepartmentDto>>> GetListAsync(
            bool? isActive = null,
            CancellationToken ct = default);

        Task<ApiResponse<DepartmentDto>> GetByIdAsync(
            int id,
            CancellationToken ct = default);

        Task<ApiResponse<object>> CreateAsync(
            DepartmentUpsertDto model,
            CancellationToken ct = default);

        Task<ApiResponse<object>> UpdateAsync(
            int id,
            DepartmentUpsertDto model,
            CancellationToken ct = default);

        Task<ApiResponse<object>> ToggleAsync(
            int id,
            CancellationToken ct = default);

        Task<ApiResponse<object>> DeleteAsync(
            int id,
            CancellationToken ct = default);
    }
}

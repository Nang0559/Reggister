

using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Departments
{
    public interface IDepartmentManagementClientService
    {
        Task<ApiResponse<DepartmentTreeViewModel>> GetTreeAsync(CancellationToken ct = default);

        Task<ApiResponse<List<DepartmentItemViewModel>>> GetListAsync(bool? isActive, CancellationToken ct = default);

        Task<ApiResponse<DepartmentEditViewModel>> GetByIdAsync(int id, CancellationToken ct = default);

        Task<ApiResponse<object>> CreateAsync(DepartmentEditViewModel model, CancellationToken ct = default);

        Task<ApiResponse<object>> UpdateAsync(DepartmentEditViewModel model, CancellationToken ct = default);

        Task<ApiResponse<object>> ToggleAsync(int id, CancellationToken ct = default);

        Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default);
    }
}

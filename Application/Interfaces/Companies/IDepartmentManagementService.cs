


using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Application.Interfaces.Companies
{
    public interface IDepartmentManagementService
    {
        Task<ServiceResult<List<DepartmentDto>>> GetAllAsync(CancellationToken ct = default);

        Task<ServiceResult<List<DepartmentDto>>> GetFilteredAsync(
            bool? isActive, CancellationToken ct = default);

        Task<ServiceResult<DepartmentDto>> GetByIdAsync(int id, CancellationToken ct = default);

        Task<ServiceResult> CreateAsync(
            DepartmentUpsertDto model, int currentUserId, CancellationToken ct = default);

        Task<ServiceResult> UpdateAsync(
            DepartmentUpsertDto model, int currentUserId, CancellationToken ct = default);

        Task<ServiceResult> ToggleActiveAsync(
            int id, int currentUserId, CancellationToken ct = default);

        /// <summary>Chặn xóa nếu còn nhân viên đang gắn DeptCode này (F03employee.DeptCode là string).</summary>
        Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
    }
}

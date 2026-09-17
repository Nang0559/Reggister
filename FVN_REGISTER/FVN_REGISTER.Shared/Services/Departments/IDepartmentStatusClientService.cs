using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Responses;



namespace FVN_REGISTER.Shared.Services.Departments
{
    public interface IDepartmentStatusClientService
    {
        Task<ApiResponse<List<DepartmentStatusDto>>> GetAllAsync(
            DateTime? date = null,
            CancellationToken ct = default);

        Task<ApiResponse<DepartmentStatusDto>> GetByDeptAsync(
            string deptCode,
            DateTime? date = null,
            CancellationToken ct = default);
    }
}

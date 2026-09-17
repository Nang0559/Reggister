using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Responses;



namespace FVN_REGISTER.Shared.Services.OTs
{
    public interface IDeptClientService
    {
        Task<ApiResponse<List<DeptOption>>> GetDepartmentsAsync(
            CancellationToken ct = default);

        // Cache trong memory để tránh gọi API nhiều lần
        Task<List<DeptOption>> GetDepartmentsCachedAsync(
            CancellationToken ct = default);
    }
}

using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Contract.Interfaces.Repositores;


namespace FVN_REGISTER.Shared.Services.Users
{
    public interface IUserManagementClientService
    {
        Task<ApiResponse<List<UserAccountDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ApiResponse<UserAccountDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
        Task<ApiResponse<object>> UpdateAsync(UpdateUserRequest request, CancellationToken ct = default);
        Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> ToggleLockAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> ResetPasswordAsync(int id, string newPassword, CancellationToken ct = default);
    }
}

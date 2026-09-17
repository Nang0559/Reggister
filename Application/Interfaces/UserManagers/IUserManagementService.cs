
using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Application.Interfaces.UserManagers
{
    public interface IUserManagementService
    {
        Task<List<UserAccountDto>> GetAllAsync(CancellationToken ct = default);
        Task<UserAccountDto?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<ServiceResult> CreateAsync(
            CreateUserRequest request, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> UpdateAsync(
            UpdateUserRequest request, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> DeleteAsync(
            int id, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> ToggleLockAsync(
            int id, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> ResetPasswordAsync(
            int id, string newPassword, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> ReactivateAsync(int id, int currentUserId, CancellationToken ct = default); // MỚI
    }
}

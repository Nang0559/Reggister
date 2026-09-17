using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Users
{
    public interface IAuthClientService
    {
        Task<ApiResponse<AuthResultDto>> Login(
            string username,
            string password,
            CancellationToken ct = default);

        Task Logout(CancellationToken ct = default);

        Task<ApiResponse<UserIdentityDto>> GetProfileAsync(CancellationToken ct = default);

        Task<ApiResponse<object>> UpdateProfileAsync(
            string email,
            string? avatarUrl,
            CancellationToken ct = default);

        Task<ApiResponse<object>> ChangePassword(
            string currentPassword,
            string newPassword,
            CancellationToken ct = default);
    }
}

using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Interfaces.Auths
{
    public interface IAuthService
    {
        Task<ServiceResult<UserSessionDto>> Login(
            string username,
            string password,
            string deviceId,        // ✅ THÊM
            string deviceType,      // ✅ THÊM
            string? deviceName,     // ✅ THÊM
            bool rememberMe,        // ✅ THÊM
            CancellationToken ct = default);

        Task<ServiceResult<UserSessionDto>> GetProfileAsync(
            int userId,
            CancellationToken ct = default);

        Task<ServiceResult> UpdateProfileAsync(
            int userId, string email, string? avatarUrl,
            CancellationToken ct = default);

        Task<ServiceResult> Logout(
            int userId,
            string? refreshToken,   // ✅ THÊM
            CancellationToken ct = default);

        Task<ServiceResult> ChangePassword(
            string username, string currentPassword,
            string newPassword,
            CancellationToken ct = default);

        // ✅ THÊM MỚI
        Task<ServiceResult<string>> RefreshTokenAsync(
            string refreshToken,
            CancellationToken ct = default);
    }
}

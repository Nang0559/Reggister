


using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Auths
{
    // IAuthService — cần thêm tham số
    public interface IAuthService
    {
        Task<ServiceResult<AuthResultDto>> Login(
            string employeeCode, string password,
            string deviceId, string deviceType, string? deviceName,
            bool rememberMe, string? ipAddress, string? userAgent,
            CancellationToken ct = default);

        Task<ServiceResult<UserIdentityDto>> GetProfileAsync(int userId, CancellationToken ct = default);
        Task<ServiceResult> UpdateProfileAsync(int userId, string email, string? avatarUrl, CancellationToken ct = default);

        Task<ServiceResult> Logout(
            int userId, string? refreshToken, string? ipAddress, string? userAgent,
            CancellationToken ct = default);

        Task<ServiceResult> ChangePassword(
            string employeeCode, string currentPassword, string newPassword, CancellationToken ct = default);

        Task<ServiceResult<string>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    }
}

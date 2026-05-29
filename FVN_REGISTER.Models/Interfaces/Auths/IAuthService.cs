using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Interfaces.Auths
{
    public interface IAuthService
    {
        // 1. Thêm CancellationToken ct = default cho tất cả các hàm
        // 2. Đảm bảo kiểu trả về luôn là Task<ServiceResult<...>>

        Task<ServiceResult<UserSessionDto>> Login(string username, string password, CancellationToken ct = default);

        Task<ServiceResult<UserSessionDto>> GetProfileAsync(int userId, CancellationToken ct = default);

        Task<ServiceResult> UpdateProfileAsync(int userId, string email, string? avatarUrl, CancellationToken ct = default);

        Task<ServiceResult> Logout(int userId, CancellationToken ct = default);

        Task<ServiceResult> ChangePassword(string username, string currentPassword, string newPassword, CancellationToken ct = default);
    }
}

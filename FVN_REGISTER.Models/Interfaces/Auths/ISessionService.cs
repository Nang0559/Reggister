using FVN_REGISTER.Contract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Interfaces.Auths
{
    public interface ISessionService
    {
        /// <summary>
        /// Tạo session mới, tự động kick device cũ nếu vượt giới hạn
        /// </summary>
        Task<string> CreateSessionAsync(
            int userId,
            string deviceId,
            string deviceType,
            string? deviceName,
            bool rememberMe,
            CancellationToken ct = default);

        /// <summary>
        /// Validate refresh token → trả về userId nếu hợp lệ
        /// </summary>
        Task<int?> ValidateRefreshTokenAsync(
            string refreshToken,
            CancellationToken ct = default);

        /// <summary>
        /// Cập nhật LastSeen cho session
        /// </summary>
        Task UpdateLastSeenAsync(
            string refreshToken,
            CancellationToken ct = default);

        /// <summary>
        /// Vô hiệu hóa session khi logout
        /// </summary>
        Task RevokeSessionAsync(
            string refreshToken,
            CancellationToken ct = default);

        // <summary>
        // Lấy danh sách session đang active của user
        // </summary>
        Task<List<UserSession>> GetActiveSessionsAsync(
            int userId,
            CancellationToken ct = default);
    }
}

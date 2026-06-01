

namespace FVN_REGISTER.Contract.Interfaces.Auths
{
    public interface ISessionService
    {
        /// <summary>
        /// Tạo / cập nhật session sau khi login thành công.
        /// Nếu vượt quá số thiết bị tối đa → đổi IsActive = 0, cập nhật RevokedAt.
        /// Trả về chuỗi định danh phiên (chính là jwtToken hoặc một Token đại diện).
        /// </summary>
        Task<List<string>> RegisterSessionAsync(int userId, string deviceType, string deviceId, string? deviceName, string jwtToken, CancellationToken ct = default);

        /// <summary>
        /// Kiểm tra phiên JwtToken còn hoạt động hay không (Dùng cho RefreshTokenAsync hoặc Middleware).
        /// Trả về UserId nếu hợp lệ, ngược lại trả về null.
        /// </summary>
        Task<int?> ValidateSessionAsync(string jwtToken, CancellationToken ct = default);

        /// <summary>Gắn connectionId SignalR khi client kết nối hub.</summary>
        Task SetConnectionIdAsync(int userId, string deviceId, string connectionId, CancellationToken ct = default);

        /// <summary>Xóa connectionId khi client ngắt kết nối.</summary>
        Task ClearConnectionIdAsync(string connectionId, CancellationToken ct = default);

        /// <summary>Lấy tất cả connectionId đang online của user (để broadcast).</summary>
        Task<List<string>> GetConnectionIdsAsync(int userId, CancellationToken ct = default);

        /// <summary>Revoke session cụ thể qua token (Dùng khi Logout).</summary>
        Task RevokeAsync(string jwtToken, CancellationToken ct = default);

        /// <summary>Revoke tất cả session của user (Ví dụ: Khi đổi mật khẩu).</summary>
        Task RevokeAllAsync(int userId, CancellationToken ct = default);
    }
}

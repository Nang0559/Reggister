

namespace FVN_REGISTER.Application.Interfaces.Users
{
    /// <summary>
    /// Ghi nhận hoạt động gần nhất của user — "đang làm gì, từ đâu" (activity/presence),
    /// KHÁC với IAuditService (security audit trail: login/logout/change password).
    /// Dùng chung mọi domain qua BaseApiController.LogActionAsync, không riêng Auth.
    /// </summary>
    public interface IUserLogService
    {
        Task UpdateLastSeenAsync(
            int userId,
            string action,
            string url,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken ct = default);
    }
}

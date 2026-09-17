using FVN_REGISTER.Contract.Dtos.Authentication;

namespace FVN_REGISTER.Application.Interfaces.Auths
{
    public record SessionValidationResult(int UserId, bool RememberMe);

    public interface ISessionService
    {
        Task<List<string>> RegisterSessionAsync(
            int userId,
            string deviceType,
            string deviceId,
            string? deviceName,
            string refreshTokenRaw,
            bool rememberMe,
            CancellationToken ct = default);

        Task<SessionValidationResult?> ValidateSessionAsync(
            string refreshTokenRaw,
            CancellationToken ct = default);

        Task RevokeAsync(string refreshTokenRaw, CancellationToken ct = default);

        Task RevokeAllAsync(int userId, CancellationToken ct = default);

        Task<List<SessionDto>> GetActiveSessionsAsync(
            int userId,
            string? currentRefreshTokenRaw,
            CancellationToken ct = default);

        Task<SessionRevocationResult?> RevokeSessionAsync(
            int userId,
            int sessionId,
            CancellationToken ct = default);
    }
}

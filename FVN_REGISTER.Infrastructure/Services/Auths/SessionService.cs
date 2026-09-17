
using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Text;



namespace FVN_REGISTER.Infrastructure.Services.Auths
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SessionService> _logger;

        private const int SessionDaysDefault = 1;
        private const int SessionDaysRemember = 30;

        public SessionService(IUnitOfWork uow, ILogger<SessionService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<List<string>> RegisterSessionAsync(
            int userId, string deviceType, string deviceId, string? deviceName,
            string refreshTokenRaw, bool rememberMe, CancellationToken ct = default)
        {
            var repo = _uow.Repository<F03UserSession>();

            var others = await repo.Query()
                .Where(x => x.UserId == userId && x.DeviceType == deviceType && x.IsActive)
                .ToListAsync(ct);

            var kickedConnectionIds = others
                .Where(x => !string.IsNullOrEmpty(x.SignalRConnectionId))
                .Select(x => x.SignalRConnectionId!)
                .ToList();

            foreach (var s in others)
            {
                s.IsActive = false;
                s.RevokedAt = DateTime.Now;
            }

            var days = rememberMe ? SessionDaysRemember : SessionDaysDefault;

            await repo.AddAsync(new F03UserSession
            {
                UserId = userId,
                DeviceType = deviceType,
                DeviceId = deviceId,
                DeviceName = deviceName,
                RememberMe = rememberMe, // lưu lại — bắt buộc để RefreshTokenAsync đọc đúng sau này
                JwtToken = HashToken(refreshTokenRaw),
                ExpiresAt = DateTime.Now.AddDays(days),
                IsActive = true,
                CreatedAt = DateTime.Now, // nhất quán múi giờ với toàn hệ thống, không dùng UtcNow
                LastSeenAt = DateTime.Now
            }, ct);

            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[SESSION] Registered {DeviceType}/{DeviceId} for User={UserId} | Kicked={KickCount}",
                deviceType, deviceId, userId, kickedConnectionIds.Count);

            return kickedConnectionIds;
        }

        public async Task<SessionValidationResult?> ValidateSessionAsync(
            string refreshTokenRaw, CancellationToken ct = default)
        {
            var hash = HashToken(refreshTokenRaw);

            var session = await _uow.Repository<F03UserSession>().Query()
                .FirstOrDefaultAsync(x => x.JwtToken == hash, ct);

            if (session == null || !session.IsActive) return null;

            if (session.ExpiresAt.HasValue && session.ExpiresAt.Value < DateTime.Now)
            {
                session.IsActive = false;
                session.RevokedAt = DateTime.Now;
                await _uow.SaveChangesAsync(ct);
                return null;
            }

            // Sliding thật: gia hạn lại kể từ BÂY GIỜ theo đúng mức đã cấp lúc đăng nhập
            var slidingDays = session.RememberMe ? SessionDaysRemember : SessionDaysDefault;
            session.ExpiresAt = DateTime.Now.AddDays(slidingDays);
            session.LastSeenAt = DateTime.Now;

            await _uow.SaveChangesAsync(ct);

            return new SessionValidationResult(session.UserId, session.RememberMe);
        }

        public async Task RevokeAsync(string refreshTokenRaw, CancellationToken ct = default)
        {
            var hash = HashToken(refreshTokenRaw);

            var session = await _uow.Repository<F03UserSession>().Query()
                .FirstOrDefaultAsync(x => x.JwtToken == hash && x.IsActive, ct);

            if (session == null) return;

            session.IsActive = false;
            session.RevokedAt = DateTime.Now;
            await _uow.SaveChangesAsync(ct);
        }

        public async Task RevokeAllAsync(int userId, CancellationToken ct = default)
        {
            var sessions = await _uow.Repository<F03UserSession>().Query()
                .Where(x => x.UserId == userId && x.IsActive)
                .ToListAsync(ct);

            foreach (var s in sessions)
            {
                s.IsActive = false;
                s.RevokedAt = DateTime.Now;
            }

            await _uow.SaveChangesAsync(ct);
        }

        private static string HashToken(string token)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }
    }
}

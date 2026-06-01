using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.API.Services.Auths
{
    public class SessionService : ISessionService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly ILogger<SessionService> _logger;

        public SessionService(
            FVNWEBAPPContext db,
            ILogger<SessionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ─── CREATE ──────────────────────────────────────────
        public async Task<string> CreateSessionAsync(
            int userId,
            string deviceId,
            string deviceType,
            string? deviceName,
            bool rememberMe,
            CancellationToken ct = default)
        {
            // 1. Lấy tất cả session active, sắp xếp cũ nhất trước
            var activeSessions = await _db.UserSessions
                .Where(x => x.UserId == userId && x.IsRevoked==false)
                .OrderBy(x => x.LastSeenAt)
                .ToListAsync(ct);

            // 2. Nếu device này đã có session → update thay vì tạo mới
            var existing = activeSessions
                .FirstOrDefault(x => x.DeviceId == deviceId);

            if (existing != null)
            {
                existing.RefreshToken = GenerateRefreshToken();
                existing.LastSeenAt = DateTime.Now;
                existing.ExpireTime = GetExpiry(deviceType, rememberMe);
                existing.DeviceName = deviceName; // Cập nhật tên thiết bị

                await _db.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "[SESSION] Updated DeviceId={DeviceId} UserId={UserId}",
                    deviceId, userId);

                return existing.RefreshToken;
            }

            // 3. Kiểm tra giới hạn theo loại device → kick cũ nhất
            var mobileCount = activeSessions
                .Count(x => x.DeviceType == DeviceType.Mobile);
            var webCount = activeSessions
                .Count(x => x.DeviceType == DeviceType.Web);

            if (deviceType == DeviceType.Mobile
                && mobileCount >= SessionPolicy.MaxMobileDevices)
            {
                var oldest = activeSessions
                    .First(x => x.DeviceType == DeviceType.Mobile);

                oldest.IsRevoked = false;

                _logger.LogWarning(
                    "[SESSION] Kicked Mobile SessionId={Id} UserId={UserId}",
                    oldest.Id, userId);
            }
            else if (deviceType == DeviceType.Web
                && webCount >= SessionPolicy.MaxWebDevices)
            {
                var oldest = activeSessions
                    .First(x => x.DeviceType == DeviceType.Web);

                oldest.IsRevoked = false;

                _logger.LogWarning(
                    "[SESSION] Kicked Web SessionId={Id} UserId={UserId}",
                    oldest.Id, userId);
            }

            // 4. Tạo session mới
            var refreshToken = GenerateRefreshToken();

            _db.UserSessions.Add(new UserSession
            {
                UserId = userId,
                DeviceId = deviceId,
                DeviceType = deviceType,
                DeviceName = deviceName,
                RefreshToken = refreshToken,
                IsRevoked = true,
                CreatedAt = DateTime.Now,
                LastSeenAt = DateTime.Now,
                ExpireTime = GetExpiry(deviceType, rememberMe)
            });

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[SESSION] Created Type={Type} UserId={UserId}",
                deviceType, userId);

            return refreshToken;
        }

        // ─── VALIDATE ────────────────────────────────────────
        public async Task<int?> ValidateRefreshTokenAsync(
            string refreshToken,
            CancellationToken ct = default)
        {
            var session = await _db.UserSessions
                .FirstOrDefaultAsync(x =>
                    x.RefreshToken == refreshToken &&
                    x.IsRevoked == true &&
                    x.ExpireTime > DateTime.Now, ct);

            if (session == null)
            {
                _logger.LogWarning(
                    "[SESSION] Invalid/Expired RefreshToken");
                return null;
            }

            // Cập nhật LastSeen
            session.LastSeenAt = DateTime.Now;
            await _db.SaveChangesAsync(ct);

            return session.UserId;
        }

        // ─── UPDATE LAST SEEN ─────────────────────────────────
        public async Task UpdateLastSeenAsync(
            string refreshToken,
            CancellationToken ct = default)
        {
            await _db.UserSessions
                .Where(x => x.RefreshToken == refreshToken && x.IsRevoked)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.LastSeenAt, DateTime.Now), ct);
        }

        // ─── REVOKE ───────────────────────────────────────────
        public async Task RevokeSessionAsync(
            string refreshToken,
            CancellationToken ct = default)
        {
            await _db.UserSessions
                .Where(x => x.RefreshToken == refreshToken)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.IsRevoked, false), ct);

            _logger.LogInformation("[SESSION] Revoked RefreshToken");
        }

        // ─── GET ACTIVE ───────────────────────────────────────
        public async Task<List<UserSession>> GetActiveSessionsAsync(
            int userId,
            CancellationToken ct = default)
            => await _db.UserSessions
                .Where(x =>
                    x.UserId == userId &&
                    x.IsRevoked == true &&
                    x.ExpireTime > DateTime.Now)
                .OrderByDescending(x => x.LastSeenAt)
                .ToListAsync(ct);

        // ─── Helpers ─────────────────────────────────────────
        private static string GenerateRefreshToken()
            => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
             + Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        private static DateTime GetExpiry(string deviceType, bool rememberMe)
        {
            if (deviceType == DeviceType.Mobile && rememberMe)
                return DateTime.Now.Add(SessionPolicy.MobileExpiry);

            return DateTime.Now.Add(SessionPolicy.WebExpiry);
        }
    }
}

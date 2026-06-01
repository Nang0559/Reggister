using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Models;

using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Core.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.API.Services.Auths
{
  

    public class SessionService : BaseService<SessionService>, ISessionService
    {
        private readonly FVNWEBAPPContext _db;
        private const int MaxDevices = 2;  // Giới hạn 2 thiết bị (ví dụ: 1 Web + 1 Mobile)

        public SessionService(
            FVNWEBAPPContext db,
            ILogger<SessionService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
        }

        // ─── REGISTER SESSION (LOGIN) ──────────────────────────
        public async Task<List<string>> RegisterSessionAsync(
            int userId, string deviceType, string deviceId,
            string? deviceName, string jwtToken, CancellationToken ct = default)
        {
            var kickedConnectionIds = new List<string>();

            // 1. Tìm session cùng deviceId (bất kể active hay không) để tái sử dụng, tránh duplicate unique index
            var existing = await _db.UserSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DeviceId == deviceId, ct);

            if (existing != null)
            {
                existing.JwtToken = jwtToken;
                existing.IsActive = true;
                existing.LastSeenAt = DateTime.Now;
                existing.RevokedAt = null;
                existing.DeviceName = deviceName;
                existing.DeviceType = deviceType;

                await _db.SaveChangesAsync(ct);

                Logger.LogDebugIf(Debug, "[SESSION] Refreshed existing session for User={UserId} Device={Device}", userId, deviceId);
                return kickedConnectionIds; // Không kick ai vì đây là cập nhật thiết bị hiện tại
            }

            // 2. Đếm và lấy danh sách session đang active
            var activeSessions = await _db.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderBy(s => s.LastSeenAt)
                .ToListAsync(ct);

            // 3. Nếu đã đạt tới giới hạn MaxDevices → tiến hành kick session cũ nhất
            while (activeSessions.Count >= MaxDevices)
            {
                var oldest = activeSessions.First();
                oldest.IsActive = false;
                oldest.RevokedAt = DateTime.Now;

                if (oldest.SignalRConnectionId != null)
                {
                    kickedConnectionIds.Add(oldest.SignalRConnectionId);
                }

                activeSessions.RemoveAt(0);

                Logger.LogWarnIf(Debug,
                    "[SESSION] Kicked oldest session User={UserId} Device={Device} ConnectionId={ConnId}",
                    userId, oldest.DeviceId, oldest.SignalRConnectionId);
            }

            // 4. Tạo phiên làm việc mới
            var session = new UserSession
            {
                UserId = userId,
                DeviceType = deviceType,
                DeviceId = deviceId,
                DeviceName = deviceName,
                JwtToken = jwtToken,
                IsActive = true,
                CreatedAt = DateTime.Now,
                LastSeenAt = DateTime.Now
            };

            _db.UserSessions.Add(session);
            await _db.SaveChangesAsync(ct);

            Logger.LogInfoIf(Debug,
                "[SESSION] New session created User={UserId} Device={Device} Type={Type}",
                userId, deviceId, deviceType);

            return kickedConnectionIds;
        }

        // ─── VALIDATE SESSION (REFRESH TOKEN) ──────────────────
        // 🌟 THÊM MỚI HÀM NÀY ĐỂ KHỚP VỚI AUTH_SERVICE
        public async Task<int?> ValidateSessionAsync(string jwtToken, CancellationToken ct = default)
        {
            var session = await _db.UserSessions
                .FirstOrDefaultAsync(s => s.JwtToken == jwtToken && s.IsActive, ct);

            if (session == null) return null;

            // Cập nhật mốc thời gian hoạt động cuối cùng của session
            session.LastSeenAt = DateTime.Now;
            await _db.SaveChangesAsync(ct);

            return session.UserId;
        }

        // ─── SET SIGNALR CONNECTION ID ────────────────────────
        public async Task SetConnectionIdAsync(int userId, string deviceId,
            string connectionId, CancellationToken ct = default)
        {
            var session = await _db.UserSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DeviceId == deviceId && s.IsActive, ct);

            if (session == null) return;

            session.SignalRConnectionId = connectionId;
            session.LastSeenAt = DateTime.Now;
            await _db.SaveChangesAsync(ct);
        }

        // ─── CLEAR SIGNALR CONNECTION ID ──────────────────────
        public async Task ClearConnectionIdAsync(string connectionId, CancellationToken ct = default)
        {
            var session = await _db.UserSessions
                .FirstOrDefaultAsync(s => s.SignalRConnectionId == connectionId, ct);

            if (session == null) return;

            session.SignalRConnectionId = null;
            await _db.SaveChangesAsync(ct);
        }

        // ─── GET CONNECTION IDS FOR BROADCAST ──────────────────
        public async Task<List<string>> GetConnectionIdsAsync(int userId, CancellationToken ct = default)
            => await _db.UserSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.IsActive && s.SignalRConnectionId != null)
                .Select(s => s.SignalRConnectionId!)
                .ToListAsync(ct);

        // ─── REVOKE SINGLE SESSION (LOGOUT) ────────────────────
        // Thay đổi param nhận vào từ (userId, deviceId) thành (jwtToken) để đồng bộ hóa việc đăng xuất từ chuỗi token
        public async Task RevokeAsync(string jwtToken, CancellationToken ct = default)
        {
            var session = await _db.UserSessions
                .FirstOrDefaultAsync(s => s.JwtToken == jwtToken, ct);

            if (session == null) return;

            session.IsActive = false;
            session.RevokedAt = DateTime.Now;
            session.SignalRConnectionId = null;
            await _db.SaveChangesAsync(ct);
        }

        // ─── REVOKE ALL SESSIONS ───────────────────────────────
        public async Task RevokeAllAsync(int userId, CancellationToken ct = default)
        {
            await _db.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsActive, false)
                    .SetProperty(n => n.RevokedAt, DateTime.Now)
                    .SetProperty(n => n.SignalRConnectionId, (string?)null), ct);
        }
    }
}

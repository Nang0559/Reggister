using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Models;

namespace FVN_REGISTER.API.Services.Auths
{
    public class AuditService : IAuditService
    {
        private readonly FVNWEBAPPContext _db;

        public AuditService(FVNWEBAPPContext db)
        {
            _db = db;
        }

        public async Task LogLoginSuccess(int userId)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = "LOGIN_SUCCESS",
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }

        public async Task LogLoginFailed(string username)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserName = username,
                Action = "LOGIN_FAILED",
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }

        public async Task LogLogout(int userId)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = "LOGOUT",
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }

        public async Task LogAction(string action, int? userId, string description)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                Description = description,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();
        }
    }
}



using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Repositories;

namespace FVN_REGISTER.Infrastructure.Services.Auths
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _uow;

        public AuditService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task LogLoginSuccess(int userId, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default)
        {
            await _uow.Repository<F03AuditLog>().AddAsync(new F03AuditLog
            {
                UserId = userId,
                Action = "LOGIN_SUCCESS",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedBy = userId
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task LogLoginFailed(string username, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default)
        {
            await _uow.Repository<F03AuditLog>().AddAsync(new F03AuditLog
            {
                UserId = null,
                UserName = username,
                Action = "LOGIN_FAILED",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedBy = SystemUser.Id
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task LogLogout(int userId, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default)
        {
            await _uow.Repository<F03AuditLog>().AddAsync(new F03AuditLog
            {
                UserId = userId,
                Action = "LOGOUT",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedBy = userId
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task LogAction(
            string action, int? userId, string description,
            string? ipAddress = null, string? userAgent = null, CancellationToken ct = default)
        {
            await _uow.Repository<F03AuditLog>().AddAsync(new F03AuditLog
            {
                UserId = userId,
                Action = action,
                Description = description,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedBy = userId ?? SystemUser.Id
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }
    }
}

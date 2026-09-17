

namespace FVN_REGISTER.Application.Interfaces.Auths
{
    public interface IAuditService
    {
        Task LogLoginSuccess(int userId, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default);
        Task LogLoginFailed(string username, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default);
        Task LogLogout(int userId, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default);
        Task LogAction(string action, int? userId, string description, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default);
    }
}



namespace FVN_REGISTER.Contract.Interfaces.Auths
{
    public interface IAuditService
    {
        Task LogLoginSuccess(int userId);
        Task LogLoginFailed(string username);
        Task LogLogout(int userId);
        Task LogAction(string action, int? userId, string description);
    }
}

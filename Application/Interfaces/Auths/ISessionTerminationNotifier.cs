namespace FVN_REGISTER.Application.Interfaces.Auths
{
    /// <summary>
    /// Application port for notifying a client that its authenticated session was revoked.
    /// SignalR is an Infrastructure concern; implementations belong outside Application/API.
    /// </summary>
    public interface ISessionTerminationNotifier
    {
        Task NotifyRevokedAsync(string connectionId, string message, CancellationToken ct = default);
    }
}

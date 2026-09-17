using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FVN_REGISTER.Infrastructure.Services.Auths
{
    public sealed class SignalRSessionTerminationNotifier : ISessionTerminationNotifier
    {
        private readonly IHubContext<NotificationHub> _hub;

        public SignalRSessionTerminationNotifier(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public Task NotifyRevokedAsync(
            string connectionId,
            string message,
            CancellationToken ct = default)
        {
            return _hub.Clients.Client(connectionId)
                .SendAsync("ForceLogout", message, ct);
        }
    }
}

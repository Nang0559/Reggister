namespace FVN_REGISTER.Infrastructure.Utils;

/// <summary>
/// SignalR group naming belongs to the infrastructure/transport adapter,
/// not to the domain core.
/// </summary>
public static class NotificationHubGroups
{
    public static string ForUser(int userId) => $"user:{userId}";
}

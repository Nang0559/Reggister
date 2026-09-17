

namespace FVN_REGISTER.Core.Utils
{
    public static class NotificationHubGroups
    {
        public static string ForUser(int userId) => $"user_{userId}";
    }
}

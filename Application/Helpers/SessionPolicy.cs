namespace FVN_REGISTER.Application.Helpers
{
    /// <summary>
    /// Application session policy values. The host is responsible for enforcing them.
    /// </summary>
    public static class SessionPolicy
    {
        public const int MaxMobileDevices = 1;
        public const int MaxWebDevices = 1;

        public static readonly TimeSpan MobileExpiry = TimeSpan.FromDays(30);
        public static readonly TimeSpan WebExpiry = TimeSpan.FromDays(1);
    }
}

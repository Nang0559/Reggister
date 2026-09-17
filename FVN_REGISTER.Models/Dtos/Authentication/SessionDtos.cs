namespace FVN_REGISTER.Contract.Dtos.Authentication
{
    public sealed record SessionDto(
        int Id,
        string DeviceType,
        string DeviceName,
        DateTime CreatedAt,
        DateTime? LastSeenAt,
        bool IsCurrent);

    public sealed record SessionRevocationResult(bool Success, string? ConnectionId);
}

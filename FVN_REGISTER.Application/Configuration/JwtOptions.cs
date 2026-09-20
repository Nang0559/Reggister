namespace FVN_REGISTER.Application.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenHours { get; set; } = 8;
    public int RememberMeDays { get; set; } = 7;
}

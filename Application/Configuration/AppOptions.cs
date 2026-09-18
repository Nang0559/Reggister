namespace FVN_REGISTER.Application.Configuration;

public sealed class AppOptions
{
    public string SiteUrl { get; set; } = string.Empty;

    public string BuildUrl(string path)
    {
        var baseUrl = SiteUrl.TrimEnd('/');
        var relativePath = path.TrimStart('/');
        return $"{baseUrl}/{relativePath}";
    }
}

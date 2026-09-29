namespace FVN_REGISTER.Shared.Services.Language;

public enum LanguageCode
{
    Vi,
    Ja
}

public static class LanguageCodeExtensions
{
    public static string ToCulture(this LanguageCode language) => language switch
    {
        LanguageCode.Ja => "ja-JP",
        _ => "vi-VN"
    };

    public static LanguageCode Parse(string? value) =>
        string.Equals(value, "ja", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "ja-JP", StringComparison.OrdinalIgnoreCase)
            ? LanguageCode.Ja
            : LanguageCode.Vi;
}

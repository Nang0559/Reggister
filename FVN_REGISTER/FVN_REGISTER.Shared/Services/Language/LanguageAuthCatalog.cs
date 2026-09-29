namespace FVN_REGISTER.Shared.Services.Language;

public static class LanguageAuthCatalog
{
    private static readonly IReadOnlyDictionary<string, (string Vi, string Ja)> Text = new Dictionary<string, (string Vi, string Ja)>(StringComparer.Ordinal)
    {
        ["auth.serverConnectionFailed"] = ("Không thể kết nối đến máy chủ FCC.", "FCCサーバーに接続できません。"),
    };

    public static bool TryGet(string key, LanguageCode language, out string value)
    {
        if (Text.TryGetValue(key, out var pair))
        {
            value = language == LanguageCode.Ja ? pair.Ja : pair.Vi;
            return true;
        }
        value = key;
        return false;
    }
}

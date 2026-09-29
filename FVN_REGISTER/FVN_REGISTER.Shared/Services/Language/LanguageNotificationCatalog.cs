namespace FVN_REGISTER.Shared.Services.Language;

public static class LanguageNotificationCatalog
{
    private static readonly IReadOnlyDictionary<string, (string Vi, string Ja)> Text = new Dictionary<string, (string Vi, string Ja)>(StringComparer.Ordinal)
    {
        ["notification.title"] = ("Thông báo", "通知"),
        ["notification.markAllRead"] = ("Đọc tất cả", "すべて既読"),
        ["notification.empty"] = ("Chưa có thông báo", "通知はありません"),
        ["notification.justNow"] = ("Vừa xong", "たった今"),
        ["notification.minutesAgo"] = ("{0} phút trước", "{0}分前"),
        ["notification.hoursAgo"] = ("{0} giờ trước", "{0}時間前"),
        ["notification.daysAgo"] = ("{0} ngày trước", "{0}日前"),
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

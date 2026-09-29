namespace FVN_REGISTER.Shared.Services.Language;

public static class LanguageLeaveCatalog
{
    private static readonly IReadOnlyDictionary<string, (string Vi, string Ja)> Text = new Dictionary<string, (string Vi, string Ja)>(StringComparer.Ordinal)
    {
        ["leave.create.title"] = ("Đăng ký nghỉ phép", "休暇申請"),
        ["leave.create.action"] = ("Đăng ký nghỉ", "休暇を申請"),
        ["leave.create.loadFailed"] = ("Không thể tải dữ liệu. Vui lòng thử lại.", "データを読み込めませんでした。もう一度お試しください。"),
        ["calendar.sharedTitle"] = ("Lịch làm việc chung", "共通勤務カレンダー"),
        ["calendar.sharedDescription"] = ("Lịch này dùng chung cho Nghỉ phép, OT và Công tác; ngày nghỉ công ty chỉ là thông tin lịch, quy tắc đăng ký cuối cùng do từng nghiệp vụ kiểm tra trên máy chủ.", "このカレンダーは休暇、残業、出張で共通利用します。会社休日はカレンダー情報であり、最終的な申請ルールは各業務のサーバー側で確認します。"),
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

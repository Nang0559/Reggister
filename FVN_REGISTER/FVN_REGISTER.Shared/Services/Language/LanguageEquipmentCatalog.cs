namespace FVN_REGISTER.Shared.Services.Language;

public static class LanguageEquipmentCatalog
{
    private static readonly IReadOnlyDictionary<string, (string Vi, string Ja)> Text = new Dictionary<string, (string Vi, string Ja)>(StringComparer.Ordinal)
    {
        ["equipment.accessDenied"] = ("Tài khoản chưa được cấp quyền Sổ quản lý thiết bị.", "このアカウントには備品管理台帳の権限がありません。"),
        ["equipment.title"] = ("Sổ quản lý thiết bị", "備品管理台帳"),
        ["equipment.register"] = ("Đăng ký thiết bị", "備品登録"),
        ["equipment.name"] = ("Tên thiết bị", "備品名"),
        ["equipment.assetCode"] = ("Mã tài sản", "資産コード"),
        ["equipment.specification"] = ("Thông số / Model", "仕様 / 型式"),
        ["equipment.serialNumber"] = ("Số Serial", "シリアル番号"),
        ["equipment.location"] = ("Vị trí", "設置場所"),
        ["equipment.price"] = ("Giá thành", "価格"),
        ["equipment.purchaseDate"] = ("Ngày mua", "購入日"),
        ["equipment.depreciationDate"] = ("Ngày khấu hao dự kiến", "予定減価償却日"),
        ["equipment.department"] = ("Bộ phận", "部門"),
        ["equipment.note"] = ("Ghi chú", "備考"),
        ["equipment.createQr"] = ("Tạo đăng ký & sinh QR", "登録してQRを発行"),
        ["equipment.qrTitle"] = ("QR thiết bị", "備品QR"),
        ["equipment.qrDescription"] = ("QR được sinh trước; chỉ có hiệu lực sau khi duyệt hoàn tất.", "QRは先に発行されますが、承認完了後に有効になります。"),
        ["equipment.submit"] = ("Gửi duyệt", "承認申請"),
        ["equipment.scan"] = ("Quét QR", "QRスキャン"),
        ["equipment.qrToken"] = ("Hoặc nhập QR token", "またはQRトークンを入力"),
        ["equipment.lookup"] = ("Tra cứu", "検索"),
        ["equipment.myRequests"] = ("Yêu cầu của tôi", "自分の申請"),
        ["equipment.kind"] = ("Loại", "種類"),
        ["equipment.status"] = ("Trạng thái", "ステータス"),
        ["equipment.selectApprover"] = ("Vui lòng chọn người phê duyệt cho từng cấp.", "各承認レベルの承認者を選択してください。"),
        ["equipment.created"] = ("Đã tạo Draft và sinh QR.", "下書きを作成し、QRを発行しました。"),
        ["equipment.createFailed"] = ("Không thể tạo đăng ký.", "登録を作成できませんでした。"),
        ["equipment.submitted"] = ("Đã gửi duyệt.", "承認申請を送信しました。"),
        ["equipment.submitFailed"] = ("Không thể gửi duyệt.", "承認申請を送信できませんでした。"),
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

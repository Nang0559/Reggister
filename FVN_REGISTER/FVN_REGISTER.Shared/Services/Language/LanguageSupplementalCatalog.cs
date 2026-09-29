namespace FVN_REGISTER.Shared.Services.Language;

/// <summary>
/// Module-specific UI vocabulary kept separate so adding translations does not require changing the core catalog.
/// Backend/API messages remain Vietnamese; this catalog is presentation-only.
/// </summary>
public static class LanguageSupplementalCatalog
{
    private static readonly IReadOnlyDictionary<string, (string Vi, string Ja)> Text =
        new Dictionary<string, (string Vi, string Ja)>(StringComparer.Ordinal)
        {
            ["home.greeting"] = ("Xin chào, {0}", "こんにちは、{0}さん"),
            ["home.description"] = ("Đây là trung tâm công việc cá nhân: theo dõi đơn, xử lý phê duyệt và các thông tin cần chú ý.", "申請、承認処理、確認が必要な情報をまとめて確認できます。"),
            ["home.workCenter"] = ("Trung tâm công việc", "ワークセンター"),
            ["home.workCenterNotice"] = ("Thông báo và phê duyệt dùng chung một Action Center. Thiết bị cũng được mở ngay trong Workspace.", "通知と承認は共通のアクションセンターで管理します。備品もこのワークスペースから確認できます。"),
            ["home.approvalDescription"] = ("Tập trung các việc đang chờ xử lý theo quyền của tài khoản.", "アカウントの権限に応じた処理待ちの項目をまとめて確認します。"),
            ["home.myLeave"] = ("Nghỉ phép của tôi", "自分の休暇"),
            ["home.myLeaveDescription"] = ("Xem số dư, lịch sử và trạng thái các đơn nghỉ.", "休暇残日数、履歴、申請状況を確認します。"),
            ["home.myOvertime"] = ("OT của tôi", "自分の残業"),
            ["home.myOvertimeDescription"] = ("Theo dõi giờ OT và các yêu cầu đã gửi.", "残業時間と申請状況を確認します。"),
            ["home.personalOverview"] = ("Tổng quan cá nhân", "個人概要"),
            ["home.updatedAt"] = ("Cập nhật {0}", "更新日時 {0}"),
            ["home.equipmentDescription"] = ("Schema theo phòng ban và Excel import nằm trong cùng workspace.", "部門別の備品情報とExcel取込を同じワークスペースで管理します。"),
            ["home.openEquipment"] = ("Mở Equipment", "備品を開く"),
            ["home.noDashboardData"] = ("Dashboard chưa có dữ liệu để hiển thị.", "表示できるダッシュボードデータがありません。"),
            ["auth.invalidSession"] = ("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.", "ログインセッションが無効または期限切れです。"),
            ["auth.sessionExpired"] = ("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "ログインセッションの有効期限が切れました。再度ログインしてください。"),
            ["message.dashboardLoadFailed"] = ("Không thể tải Dashboard.", "ダッシュボードを読み込めませんでした。"),
            ["message.dashboardLoadFailedWithReason"] = ("Không thể tải Dashboard: {0}", "ダッシュボードを読み込めませんでした: {0}"),
            ["password.title"] = ("Đổi mật khẩu", "パスワード変更"),
            ["password.current"] = ("Mật khẩu hiện tại", "現在のパスワード"),
            ["password.new"] = ("Mật khẩu mới", "新しいパスワード"),
            ["password.confirm"] = ("Xác nhận mật khẩu mới", "新しいパスワードの確認"),
            ["password.update"] = ("Cập nhật", "更新"),
            ["password.confirmMismatch"] = ("Mật khẩu mới và xác nhận không khớp!", "新しいパスワードと確認用パスワードが一致しません。"),
            ["password.success"] = ("Đổi mật khẩu thành công. Vui lòng đăng nhập lại!", "パスワードを変更しました。再度ログインしてください。"),
            ["password.failed"] = ("Không thể đổi mật khẩu.", "パスワードを変更できませんでした。"),
            ["password.systemError"] = ("Lỗi hệ thống khi đổi mật khẩu.", "パスワード変更中にシステムエラーが発生しました。"),
            ["hrm.review.title"] = ("Rà soát đồng bộ HRM", "HRM同期レビュー"),
            ["hrm.review.heading"] = ("HRM Sync — Xử lý xung đột", "HRM同期 — 競合処理"),
            ["hrm.review.description"] = ("Danh sách flag chưa xử lý từ quá trình đồng bộ HRM. Resolve chỉ đóng flag, không tự ghi đè dữ liệu nguồn.", "HRM同期で発生した未処理フラグの一覧です。解決しても元データを自動で上書きしません。"),
            ["hrm.review.manageSync"] = ("Quản lý đồng bộ", "同期管理"),
            ["hrm.review.pendingCount"] = ("Còn {0} conflict chưa giải quyết.", "未解決の競合が {0} 件あります。"),
            ["hrm.review.empty"] = ("Không có conflict HRM đang chờ xử lý.", "処理待ちのHRM競合はありません。"),
            ["hrm.review.entity"] = ("Đối tượng", "エンティティ"),
            ["hrm.review.key"] = ("Khóa", "キー"),
            ["hrm.review.flagType"] = ("Loại flag", "フラグ種別"),
            ["hrm.review.detectedAt"] = ("Thời điểm phát hiện", "検出日時"),
            ["hrm.review.resolve"] = ("Đã xử lý", "処理済みにする"),
            ["hrm.review.loadFailed"] = ("Không tải được danh sách conflict HRM.", "HRM競合一覧を読み込めませんでした。"),
            ["hrm.review.resolveFailed"] = ("Không thể giải quyết flag.", "フラグを解決できませんでした。"),
            ["hrm.review.resolveSuccess"] = ("Đã đánh dấu conflict là đã xử lý.", "競合を処理済みにしました。"),
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

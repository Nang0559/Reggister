namespace FVN_REGISTER.Shared.Services.Language;

/// <summary>
/// Central UI vocabulary. Business codes, database values and API enum values must never be localized.
/// </summary>
public static class LanguageCatalog
{
    private static readonly IReadOnlyDictionary<string, (string Vi, string Ja)> Text =
        new Dictionary<string, (string Vi, string Ja)>(StringComparer.Ordinal)
        {
            ["app.name"] = ("Hệ thống đăng ký nội bộ", "社内申請システム"),
            ["login.employeeCode"] = ("Mã nhân viên / Tên đăng nhập", "社員コード / ユーザー名"),
            ["login.password"] = ("Mật khẩu", "パスワード"),
            ["login.language"] = ("Ngôn ngữ", "言語"),
            ["login.vietnamese"] = ("Tiếng Việt", "ベトナム語"),
            ["login.japanese"] = ("Tiếng Nhật", "日本語"),
            ["login.submit"] = ("ĐĂNG NHẬP", "ログイン"),
            ["login.authenticating"] = ("Đang xác thực...", "認証中..."),
            ["login.forgotPassword"] = ("Quên mật khẩu? Liên hệ phòng IT.", "パスワードを忘れた場合はIT部門へ連絡してください。"),
            ["login.publicInfo"] = ("Thông báo & quy định công khai", "お知らせ・公開規程"),
            ["login.success"] = ("Xác thực thành công!", "認証に成功しました。"),
            ["login.invalidCredentials"] = ("Mã nhân viên hoặc mật khẩu không đúng", "社員コードまたはパスワードが正しくありません。"),
            ["common.save"] = ("Lưu", "保存"), ["common.cancel"] = ("Hủy", "キャンセル"),
            ["common.close"] = ("Đóng", "閉じる"), ["common.search"] = ("Tìm kiếm", "検索"),
            ["common.refresh"] = ("Làm mới", "更新"), ["common.edit"] = ("Sửa", "編集"),
            ["common.delete"] = ("Xóa", "削除"), ["common.approve"] = ("Phê duyệt", "承認"),
            ["common.reject"] = ("Từ chối", "却下"), ["common.submit"] = ("Gửi", "送信"),
            ["common.loading"] = ("Đang tải...", "読み込み中..."), ["common.yes"] = ("Có", "はい"),
            ["common.no"] = ("Không", "いいえ"), ["common.all"] = ("Tất cả", "すべて"),
            ["common.status"] = ("Trạng thái", "ステータス"), ["common.note"] = ("Ghi chú", "備考"),
            ["common.error"] = ("Lỗi", "エラー"), ["common.success"] = ("Thành công", "完了"),
            ["nav.home"] = ("Trang chủ", "ホーム"), ["nav.calendar"] = ("Lịch công", "勤務カレンダー"),
            ["nav.leave"] = ("Nghỉ phép", "休暇申請"), ["nav.overtime"] = ("Làm thêm giờ (OT)", "残業申請 (OT)"),
            ["nav.trip"] = ("Công tác", "出張申請"), ["nav.equipment"] = ("Thiết bị", "備品"),
            ["nav.notifications"] = ("Thông báo", "通知"), ["nav.tasks"] = ("Việc cần làm", "タスク"),
            ["nav.payroll"] = ("Tính lương", "給与計算"), ["nav.approvals"] = ("Phê duyệt", "承認"),
            ["nav.data"] = ("Dữ liệu", "データ"), ["nav.reports"] = ("Báo cáo & thống kê", "レポート・統計"),
            ["nav.system"] = ("Hệ thống", "システム"), ["nav.create"] = ("Tạo đơn mới", "新規申請"),
            ["nav.history"] = ("Lịch sử", "履歴"), ["nav.pendingApproval"] = ("Danh sách chờ duyệt", "承認待ち一覧"),
            ["nav.department"] = ("Bộ phận", "部門"), ["nav.employee"] = ("Nhân viên", "社員"),
            ["nav.approver"] = ("Người phê duyệt", "承認者"), ["nav.leaveType"] = ("Hình thức nghỉ", "休暇種別"),
            ["nav.hrmSync"] = ("Đồng bộ HRM", "HRM同期"), ["nav.hrmConflict"] = ("Xung đột HRM", "HRM競合"),
            ["nav.hrmRoleRule"] = ("Quy tắc vai trò HRM → User", "HRMロール → ユーザー規則"),
            ["nav.approvalPolicy"] = ("Chính sách phê duyệt", "承認ポリシー"), ["nav.attendanceCalculation"] = ("Tính giờ theo HRM", "HRM勤務時間計算"),
            ["nav.otList"] = ("Danh sách đơn OT", "OT申請一覧"), ["nav.otLimit"] = ("Hạn mức OT", "OT上限"),
            ["nav.reportCenter"] = ("Trung tâm báo cáo", "レポートセンター"), ["nav.attendance"] = ("Chấm công", "勤怠"),
            ["nav.administrator"] = ("Quản trị viên", "管理者"), ["nav.securityCenter"] = ("Trung tâm bảo mật", "セキュリティセンター"),
            ["nav.devices"] = ("Thiết bị & phiên đăng nhập", "端末・ログインセッション"), ["nav.emailTemplates"] = ("Mẫu email", "メールテンプレート"),
            ["nav.emailQueue"] = ("Email chờ gửi", "送信待ちメール"), ["nav.workCalendar"] = ("Năm & ngày nghỉ", "年度・休日"),
            ["payroll.attendance"] = ("Bảng công tính lương", "給与計算用勤務表"), ["payroll.prepare"] = ("Chuẩn bị bảng công", "勤務表を準備"),
            ["payroll.print"] = ("In bảng công", "勤務表を印刷"), ["payroll.lock"] = ("Khóa kỳ lương", "給与期間を確定"),
            ["payroll.period"] = ("Kỳ lương", "給与期間"),
            ["execution.confirmation"] = ("Xác nhận công", "勤務実績の確認"), ["execution.evidence"] = ("Bằng chứng làm việc", "勤務証拠"),
            ["execution.hrReview"] = ("Nhân sự xử lý xác nhận công", "人事による勤務実績確認"), ["execution.ok"] = ("Đúng", "正しい"),
            ["execution.ng"] = ("Không đúng", "不正確"), ["execution.pending"] = ("Chờ xử lý", "処理待ち")
        };

    public static string Get(string key, LanguageCode language) =>
        Text.TryGetValue(key, out var value) ? (language == LanguageCode.Ja ? value.Ja : value.Vi) : key;
}

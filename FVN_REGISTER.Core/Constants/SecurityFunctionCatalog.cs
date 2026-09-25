namespace FVN_REGISTER.Core.Constants;

/// <summary>
/// Tên và mô tả mặc định của chức năng dành cho giao diện quản trị.
/// FunctionKey vẫn là mã kỹ thuật ổn định; nội dung hiển thị được quản lý riêng để sẵn sàng mở rộng đa ngôn ngữ.
/// </summary>
public static class SecurityFunctionCatalog
{
    private static readonly IReadOnlyDictionary<string, string> ModuleNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Dashboard"] = "Bảng điều khiển",
            ["Leave"] = "Nghỉ phép",
            ["OT"] = "Làm thêm giờ",
            ["Trip"] = "Công tác",
            ["Equipment"] = "Thiết bị",
            ["UserManagement"] = "Quản lý người dùng",
            ["HrmSync"] = "Đồng bộ nhân sự",
            ["Security"] = "Bảo mật",
            ["PublicInformation"] = "Thông tin công khai",
            ["PublicForm"] = "Biểu mẫu công khai",
            ["Execution"] = "Xử lý đối sách",
            ["Payroll"] = "Bảng lương",
            ["Attendance"] = "Chấm công",
            ["Department"] = "Phòng ban",
            ["Employee"] = "Nhân viên",
            ["LeaveType"] = "Loại nghỉ phép",
            ["Approver"] = "Người duyệt",
            ["WorkCalendar"] = "Lịch làm việc",
            ["Calendar"] = "Lịch cá nhân",
            ["DepartmentStatus"] = "Trạng thái phòng ban",
            ["OTLimit"] = "Giới hạn làm thêm giờ",
            ["ApprovalPolicy"] = "Chính sách phê duyệt",
            ["HrmUserRoleRule"] = "Quy tắc vai trò người dùng nhân sự",
            ["EmailQueue"] = "Hàng đợi thư điện tử",
            ["EmailTemplate"] = "Mẫu thư điện tử",
            ["SecurityAccessChange"] = "Thay đổi quyền truy cập"
        };

    private static readonly IReadOnlyDictionary<string, string> ActionNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["View"] = "Xem",
            ["Create"] = "Thêm",
            ["Edit"] = "Sửa",
            ["Delete"] = "Xóa",
            ["Cancel"] = "Hủy",
            ["Approve"] = "Phê duyệt",
            ["Export"] = "Xuất dữ liệu",
            ["Import"] = "Nhập dữ liệu",
            ["Reconcile"] = "Đối chiếu",
            ["Repair"] = "Lập phiếu sửa chữa",
            ["Assign"] = "Bàn giao",
            ["Transfer"] = "Điều chuyển",
            ["Return"] = "Thu hồi",
            ["Liquidate"] = "Thanh lý",
            ["QR"] = "Mã QR",
            ["History"] = "Lịch sử",
            ["InspectionManage"] = "Quản lý kiểm kê",
            ["InspectionExecute"] = "Thực hiện kiểm kê",
            ["InspectionApprove"] = "Phê duyệt kiểm kê",
            ["InspectionReport"] = "Báo cáo kiểm kê",
            ["Manage"] = "Quản lý",
            ["ManageRoles"] = "Quản lý vai trò",
            ["ManageFunctions"] = "Quản lý chức năng",
            ["Audit"] = "Kiểm tra nhật ký",
            ["ResetPassword"] = "Yêu cầu cấp lại mật khẩu",
            ["Lock"] = "Khóa tài khoản",
            ["AssignPermission"] = "Gán quyền",
            ["ManageTwoFactor"] = "Quản lý xác thực hai bước",
            ["ViewStatus"] = "Xem trạng thái",
            ["SubmissionView"] = "Xem phiếu gửi",
            ["Sync"] = "Đồng bộ",
            ["Review"] = "Rà soát",
            ["Retry"] = "Thử lại",
            ["Prepare"] = "Chuẩn bị",
            ["Calculate"] = "Tính toán",
            ["Feedback"] = "Phản hồi",
            ["Execute"] = "Thực hiện",
            ["Report"] = "Báo cáo",
            ["ExportExcel"] = "Xuất Excel",
            ["ImportExcel"] = "Nhập Excel"
        };

    public static string GetDisplayName(string functionKey) => GetDisplayName(functionKey, "vi-VN");

    public static string GetDisplayName(string functionKey, string languageCode)
    {
        if (!string.Equals(languageCode, "vi-VN", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Ngôn ngữ chức năng '{languageCode}' chưa được đăng ký.");
        if (string.IsNullOrWhiteSpace(functionKey)) return "Chức năng chưa xác định";
        var parts = functionKey.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
        var module = parts.Length > 0 && ModuleNames.TryGetValue(parts[0], out var moduleName)
            ? moduleName
            : "Chức năng chưa được đặt tên";
        if (parts.Length == 1) return module;
        var action = ActionNames.TryGetValue(parts[1], out var actionName)
            ? actionName
            : "Thao tác chưa được đặt tên";
        return $"{action} {module.ToLowerInvariant()}";
    }

    public static string GetDescription(string functionKey) => GetDescription(functionKey, "vi-VN");

    public static string GetDescription(string functionKey, string languageCode)
    {
        var name = GetDisplayName(functionKey, languageCode);
        return $"Cho phép người dùng {name.ToLowerInvariant()} theo quyền và phạm vi dữ liệu được cấp.";
    }
}

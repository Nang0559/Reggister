

namespace FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto
{
    /// <summary>
    /// Đại diện cho cấu hình một bước duyệt tại thời điểm tạo đơn.
    /// Dữ liệu này dùng để lưu snapshot vào F03ApprovalSteps.
    /// </summary>
    public sealed record ApprovalStepSnapshotDto(
        int Level,
        string LevelName,      // VD: "Trưởng phòng", "Giám đốc"
        string RoleName,       // VD: "Manager", "Director"
        string? ApproverCode,  // Mã NV được gán duyệt
        string? ApproverName,  // Tên NV tại thời điểm tạo
        string? ApproverEmail, // Bắt buộc để gửi Notification
        bool IsRequired        // Nếu false, bước này có thể bỏ qua nếu không tìm thấy người duyệt
    );
}

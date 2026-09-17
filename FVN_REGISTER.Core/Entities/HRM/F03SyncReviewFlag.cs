

namespace FVN_REGISTER.Core.Entities.HRM
{
    /// <summary>
    /// Cảnh báo khi HrmSyncJob phát hiện thay đổi từ HRM có thể làm dữ liệu Admin đã cấu hình
    /// (F03Approver...) bị lệch/lỗi thời — sync KHÔNG tự sửa F03Approver (đó là quyết định
    /// nghiệp vụ cần con người), chỉ ghi cờ để Admin UI hiển thị và xử lý thủ công.
    /// </summary>
    public class F03SyncReviewFlag
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;   // "Employee" | "Department"
        public string EntityKey { get; set; } = string.Empty;
        public string FlagType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; } = DateTime.Now;
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
    }
}

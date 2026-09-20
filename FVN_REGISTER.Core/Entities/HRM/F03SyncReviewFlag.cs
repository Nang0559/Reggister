

using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.HRM
{
    /// <summary>
    /// Cảnh báo khi HrmSyncJob phát hiện thay đổi từ HRM có thể làm dữ liệu Admin đã cấu hình
    /// (F03Approver...) bị lệch/lỗi thời — sync KHÔNG tự sửa F03Approver (đó là quyết định
    /// nghiệp vụ cần con người), chỉ ghi cờ để Admin UI hiển thị và xử lý thủ công.
    /// </summary>
    [Table("F03SyncReviewFlag")]
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
        public int? CurrentApproverId { get; set; }
        public string? OldDeptCode { get; set; }
        public string? OldPositionCode { get; set; }
        public string? NewDeptCode { get; set; }
        public string? NewPositionCode { get; set; }
        public string? CurrentApproverCode { get; set; }
        public int? CurrentLevel { get; set; }
        public string? CurrentRoleName { get; set; }
        public string? CurrentApproveForDeptCode { get; set; }
        public string? SuggestedApproverCode { get; set; }
        public int? SuggestedLevel { get; set; }
        public string? SuggestedRoleName { get; set; }
        public string? SuggestedApproveForDeptCode { get; set; }
        public string? Decision { get; set; }
    }
}

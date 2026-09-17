using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Extensions
{
    public static class ApprovalStatusExtensions
    {
        

        // 2. Giữ lại ToDisplayName để lấy text (tránh trùng lặp với UIStyle)
        public static string ToDisplayName(this ApprovalStatus status) => status switch
        {
            ApprovalStatus.Draft => "Nháp",
            ApprovalStatus.Pending => "Chờ duyệt",
            ApprovalStatus.InProgress => "Đang xử lý",
            ApprovalStatus.Approved => "Đã duyệt",
            ApprovalStatus.Rejected => "Từ chối",
            ApprovalStatus.Cancelled => "Đã hủy",
            ApprovalStatus.Escalated => "Cảnh báo",
            _ => status.ToString()
        };

        // 3. Logic nghiệp vụ (Giữ nguyên)
        public static bool CanBeEdited(this ApprovalStatus status)
            => status == ApprovalStatus.Draft || status == ApprovalStatus.Rejected;

        public static bool IsFinished(this ApprovalStatus status)
            => status == ApprovalStatus.Approved ||
               status == ApprovalStatus.Rejected ||
               status == ApprovalStatus.Cancelled;
        public static bool IsInProcess(this ApprovalStatus status) => !status.IsFinished();
    }
}

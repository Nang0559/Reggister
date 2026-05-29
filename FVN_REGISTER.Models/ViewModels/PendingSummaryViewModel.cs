

namespace FVN_REGISTER.Contract.ViewModels
{
    public class PendingSummaryViewModel
    {
        // Tổng số phiếu đang chờ (tất cả các cấp)
        public int TotalPending => Level1Count + Level2Count + Level3Count;

        public int Level1Count { get; set; }
        public int Level2Count { get; set; }
        public int Level3Count { get; set; }

        // Có thể thêm danh sách chi tiết nếu Dashboard cần hiển thị list
        public List<PendingApprovalGroup> Details { get; set; } = new();
    }
}

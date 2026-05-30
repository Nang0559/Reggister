using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Interfaces.Leaves
{
    public interface ILeaveQueryService
    {
        // ================= NHÓM NGHIỆP VỤ ĐĂNG KÝ (HEAVY DATA) =================
        // Dùng cho trang đăng ký phép, lịch cá nhân
        Task<CombinedHolidaysViewModel> GetCombinedDataAsync(string empCode, string depart, string cvCode, int year, CancellationToken ct = default);

        // Dùng khi Manager bấm vào xem chi tiết danh sách đơn để duyệt
        Task<List<PendingApprovalGroup>> GetPendingDetailsAsync(string employeeCode, CancellationToken ct = default);

        // ================= NHÓM DASHBOARD (LIGHTWEIGHT DTO) =================

        // Trả về danh sách các thẻ số liệu (Duyệt phép, Vắng mặt, Cảnh báo...)
        Task<List<WidgetCounterDto>> GetDashboardWidgetsAsync(string employeeCode, string deptCode, CancellationToken ct = default);

        // Trả về top 5 đơn gần đây của chính user đó
        Task<List<RecentLeaveRequestDto>> GetRecentHistoryAsync(string employeeCode, int limit = 5, CancellationToken ct = default);

        // Trả về con số tóm tắt: Tổng phép - Đã dùng - Còn lại
        Task<LeaveBalanceDto> GetSimpleBalanceAsync(string employeeCode, int year, CancellationToken ct = default);

        // (Tùy chọn) Nếu Dashboard cần hiện số lượng đơn chờ duyệt nhanh mà không cần Icon/Color
        Task<List<PendingApprovalGroup>> GetPendingSummaryAsync(string employeeCode, CancellationToken ct = default);
    }
}

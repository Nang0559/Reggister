using FVN_REGISTER.Contract.Dtos.OT;


namespace FVN_REGISTER.Contract.Dtos.Dashboard
{
    /// <summary>
    /// Mirror cấu trúc LeaveDashboardDto cho module Overtime.
    /// Personal View: dùng OTBalanceDto/OTSummaryDto đã có sẵn.
    /// Management View: CHƯA có OT-equivalent của AbsenceWarningDto/LeaveStatisticsDto
    /// trong các Dto bạn gửi -> tạm dùng danh sách OTBalanceDto của nhân viên sắp chạm giới
    /// hạn giờ làm phần cảnh báo phòng ban (đề xuất, không phải đã có sẵn type OTStatisticsDto).
    /// Nếu bạn có/định nghĩa OTStatisticsDto tương đương LeaveStatisticsDto, gửi tôi để thay.
    /// </summary>
    public class OTDashboardDto
    {
        // Thông tin cơ bản
        public List<WidgetCounterDto> Widgets { get; set; } = new();

        // ================= Personal View =================
        public OTBalanceDto? PersonalBalance { get; set; }
        public List<OTSummaryDto> RecentRequests { get; set; } = new();

        // ================= Management View (Tách riêng để không load thừa) =================
        // Đề xuất tạm thời - danh sách nhân viên trong phòng đang gần/vượt giới hạn giờ OT,
        // đóng vai trò tương đương AbsenceWarningDto bên Leave. Cần bạn xác nhận có đúng
        // ý đồ nghiệp vụ không, hay bạn muốn 1 DTO tổng hợp riêng (OTStatisticsDto).
        public List<OTBalanceDto> NearLimitEmployees { get; set; } = new();

        // KHÔNG đưa OTWorkerStatusDto vào đây - đó là trạng thái job đồng bộ nền
        // (OTSyncBackgroundWorker/IOTSyncService), thuộc trang giám sát Admin riêng,
        // không phải dữ liệu "dashboard cá nhân/phòng ban" của nhân viên.

        // Config/Flags
        // ⚠️ Chưa rõ ý nghĩa "IsEPL" bên LeaveDashboardDto (Enterprise Payroll Link?
        // Extended Paid Leave?) nên chưa chắc OT có flag tương đương - để trống,
        // bạn xác nhận nếu cần bổ sung.
    }
}

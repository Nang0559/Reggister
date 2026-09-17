using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Contract.Dtos.Dashboard
{
    /// <summary>
    /// Kết quả 1 module (Leave/OT/Trip) đóng góp cho Dashboard tổng.
    /// Widgets: dùng để gộp chung vào hàng widget đầu trang.
    /// Detail: payload chi tiết riêng của module (vd LeaveDashboardDto, OTDashboardDto) -
    /// DashboardOrchestrator sẽ ép kiểu theo Module để gán vào đúng field của DashboardResponse.
    /// </summary>
    public class ModuleDashboardContribution
    {
        public RequestModule Module { get; set; }
        public List<WidgetCounterDto> Widgets { get; set; } = new();
        public object? Detail { get; set; }
    }
}

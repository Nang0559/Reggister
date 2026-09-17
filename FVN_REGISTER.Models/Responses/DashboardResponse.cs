using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Dashboard;


namespace FVN_REGISTER.Contract.Responses
{
    public class DashboardResponse
    {
        public bool ShowManagerView { get; set; }

        // Widget đã được DashboardWidgetPolicy sắp xếp, gộp từ mọi module
        public List<WidgetCounterDto> Widgets { get; set; } = new();

        // Đơn chờ duyệt xuyên suốt Leave/OT/Trip - lấy 1 lần từ IApprovalInboxService,
        // KHÔNG tự query riêng từng module ở Dashboard nữa.
        public List<PendingApprovalGroupDto> PendingApprovals { get; set; } = new();

        public LeaveDashboardDto? Leave { get; set; }
        public OTDashboardDto? OT { get; set; }

        // TODO: đổi thành TripDashboardDto khi module Trip có Query/Statistics service
        public object? Trip { get; set; }
    }
}

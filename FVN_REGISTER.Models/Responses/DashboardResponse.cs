using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Actions;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.Equipment;

namespace FVN_REGISTER.Contract.Responses
{
    /// <summary>
    /// HTTP/application response for the dashboard. It is the composition boundary;
    /// module details remain module DTOs and no EF entity crosses the boundary.
    /// </summary>
    public sealed class DashboardResponse
    {
        public bool ShowManagerView { get; set; }

        public List<WidgetCounterDto> Widgets { get; set; } = new();

        /// <summary>
        /// Shared Action inbox projected onto the authenticated user's Dashboard.
        /// Action remains workflow/projection state; it is not the business source-of-truth.
        /// </summary>
        public ActionCountDto ActionCount { get; set; } = new();

        public List<ActionItemDto> Actions { get; set; } = new();

        public List<PendingApprovalGroupDto> PendingApprovals { get; set; } = new();

        public LeaveDashboardDto? Leave { get; set; }

        public OTDashboardDto? OT { get; set; }

        public AbsenceWarningDto? DeptWarning { get; set; }

        public List<LeaveStatisticsDto> DepartmentStatistics { get; set; } = new();

        public object? Trip { get; set; }

        /// <summary>Recent equipment requests contributed by the Equipment dashboard provider.</summary>
        public List<EquipmentRequestDto> Equipment { get; set; } = new();
    }
}

using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;


namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class OTDashboardViewModel
    {
        public CurrentUser? CurrentUser { get; set; }
        public List<WidgetCounterDto> Widgets { get; set; } = new();
        public List<OTRequestViewModel> RecentRequests { get; set; } = new();
        public List<OTRequestViewModel> PendingApprovals { get; set; } = new();
        public OTBalanceDto? MyBalance { get; set; }
    }
}

using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Application.Policies;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Orchestrators
{
    /// <summary>
    /// Application-level composition of dashboard contributions and approval inbox data.
    /// It does not know about EF, DbContext or Infrastructure implementations.
    /// </summary>
    public class DashboardOrchestrator : IDashboardOrchestrator
    {
        private readonly IEnumerable<IModuleDashboardProvider> _providers;
        private readonly IApprovalInboxService _approvalInbox;

        public DashboardOrchestrator(
            IEnumerable<IModuleDashboardProvider> providers,
            IApprovalInboxService approvalInbox)
        {
            _providers = providers;
            _approvalInbox = approvalInbox;
        }

        public async Task<ServiceResult<DashboardResponse>> BuildAsync(
            UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                var response = new DashboardResponse
                {
                    ShowManagerView = user.Permission.IsApprover()
                };

                var rawWidgets = new List<WidgetCounterDto>();

                foreach (var provider in _providers)
                {
                    var contribution = await provider.GetContributionAsync(user, ct);
                    rawWidgets.AddRange(contribution.Widgets);

                    switch (contribution.Module)
                    {
                        case RequestModule.Leave:
                            response.Leave = contribution.Detail as LeaveDashboardDto;
                            break;
                        case RequestModule.Overtime:
                            response.OT = contribution.Detail as OTDashboardDto;
                            break;
                        case RequestModule.Trip:
                            response.Trip = contribution.Detail;
                            break;
                    }
                }

                response.Widgets = DashboardWidgetPolicy.Arrange(user, rawWidgets);

                if (response.ShowManagerView)
                {
                    var pendingResult = await _approvalInbox.GetPendingAsync(user, ct);
                    response.PendingApprovals = pendingResult.IsSuccess
                        ? pendingResult.Data ?? new List<PendingApprovalGroupDto>()
                        : new List<PendingApprovalGroupDto>();
                }

                return ServiceResult<DashboardResponse>.Ok(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<DashboardResponse>.Fail($"Không thể tải Dashboard: {ex.Message}");
            }
        }
    }
}

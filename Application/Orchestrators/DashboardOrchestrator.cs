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
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Orchestrators
{
    /// <summary>
    /// Nơi duy nhất lắp ráp Dashboard tổng. Orchestrator không biết chi tiết
    /// nghiệp vụ của từng module; module tự đóng góp qua provider.
    /// </summary>
    public sealed class DashboardOrchestrator : IDashboardOrchestrator
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
            if (string.IsNullOrWhiteSpace(user.EmployeeCode))
                return ServiceResult<DashboardResponse>.Fail(
                    "Tài khoản chưa liên kết với hồ sơ nhân viên.");

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
                        case RequestModule.Leave when contribution.Detail is LeaveDashboardDto leave:
                            response.Leave = leave;
                            response.DeptWarning = leave.DeptWarning;
                            response.DepartmentStatistics = leave.DepartmentStatistics;
                            break;

                        case RequestModule.Overtime when contribution.Detail is OTDashboardDto overtime:
                            response.OT = overtime;
                            break;

                        case RequestModule.Trip:
                            response.Trip = contribution.Detail;
                            break;

                        default:
                            // A provider may intentionally return no typed detail.
                            break;
                    }
                }

                response.Widgets = DashboardWidgetPolicy.Arrange(user, rawWidgets);

                // Pending inbox is useful only to users who can approve.
                if (response.ShowManagerView)
                {
                    var pendingResult = await _approvalInbox.GetPendingAsync(user, ct);
                    response.PendingApprovals = pendingResult.IsSuccess
                        ? pendingResult.Data ?? new List<PendingApprovalGroupDto>()
                        : new List<PendingApprovalGroupDto>();
                }

                return ServiceResult<DashboardResponse>.Ok(response);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ServiceResult<DashboardResponse>.Fail(
                    "Không thể tải Dashboard.");
            }
        }
    }
}

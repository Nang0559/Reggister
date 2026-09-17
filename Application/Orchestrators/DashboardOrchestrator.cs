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
    /// Nơi DUY NHẤT lắp ráp Dashboard tổng. Không biết chi tiết nghiệp vụ của từng module -
    /// chỉ biết lặp qua IModuleDashboardProvider, gọi IApprovalInboxService lấy pending
    /// đa module, rồi nhờ DashboardWidgetPolicy sắp xếp. Thêm module mới KHÔNG sửa file này.
    /// </summary>
    /// <summary>
    /// Nơi DUY NHẤT lắp ráp Dashboard tổng. Không biết chi tiết nghiệp vụ của từng module -
    /// chỉ biết lặp qua IModuleDashboardProvider, gọi IApprovalInboxService lấy pending
    /// đa module, rồi nhờ DashboardWidgetPolicy sắp xếp. Thêm module mới KHÔNG sửa file này.
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

        public async Task<ServiceResult<DashboardResponse>> BuildAsync(UserIdentityDto user, CancellationToken ct = default)
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

                // Chỉ hiện pending inbox cho người có quyền duyệt — tránh gọi thừa cho nhân viên thường
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

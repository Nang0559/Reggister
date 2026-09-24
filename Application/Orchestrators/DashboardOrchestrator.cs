using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Application.Interfaces.Dashboards;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Policies;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Utils;
using Microsoft.Extensions.Logging;

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
        private readonly IActionItemService _actions;
        private readonly IAuthorizationService _authorization;
        private readonly ILogger<DashboardOrchestrator> _logger;

        public DashboardOrchestrator(
            IEnumerable<IModuleDashboardProvider> providers,
            IApprovalInboxService approvalInbox,
            IActionItemService actions,
            ILogger<DashboardOrchestrator> logger,
            IAuthorizationService authorization)
        {
            _providers = providers;
            _approvalInbox = approvalInbox;
            _actions = actions;
            _logger = logger;
            _authorization = authorization;
        }

        public async Task<ServiceResult<DashboardResponse>> BuildAsync(
            UserIdentityDto user, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(user.EmployeeCode))
                return ServiceResult<DashboardResponse>.Fail(
                    "Tài khoản chưa liên kết với hồ sơ nhân viên.");

            try
            {
                var managedScopes = await _authorization.GetManagedScopesAsync(user.UserId);
                var response = new DashboardResponse
                {
                    // Manager Workspace is a data-scope concept, never an approval-role shortcut.
                    // Approval inbox is resolved independently by F03ApprovalPolicies below.
                    ShowManagerView = managedScopes.Count > 0
                };

                if (!await _authorization.HasAsync(user, SecurityFunctionCodes.DashboardView, ct))
                    return ServiceResult<DashboardResponse>.Fail("Tài khoản chưa được cấp quyền xem Dashboard.");

                var rawWidgets = new List<WidgetCounterDto>();

                // Providers share the request-scoped UnitOfWork/DbContext.
                // EF Core DbContext is not thread-safe, so providers must execute
                // sequentially within this scope.
                foreach (var provider in _providers)
                {
                    if (!await _authorization.HasAsync(user, provider.RequiredFunctionCode, ct))
                        continue;

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

                        case RequestModule.Equipment when contribution.Detail is List<EquipmentRequestDto> equipment:
                            response.Equipment = equipment;
                            break;

                        default:
                            _logger.LogWarning(
                                "[DASHBOARD] Provider returned unsupported module/detail type: {Module} / {DetailType}",
                                contribution.Module,
                                contribution.Detail?.GetType().Name ?? "null");
                            break;
                    }
                }

                response.Widgets = DashboardWidgetPolicy.Arrange(user, rawWidgets);

                // Shared Action inbox is part of the authenticated user's dashboard.
                // Action status does not replace any business workflow status.
                var actionCount = await _actions.GetCountAsync(user.EmployeeCode, user.UserId, ct);
                var actions = await _actions.GetMineAsync(user.EmployeeCode, user.UserId, includeCompleted: false, ct);
                response.ActionCount = actionCount;
                response.Actions = actions.Take(5).ToList();

                // Approval inbox is independent from Manager Workspace.
                // A user may be an approver without ManagedScope; the inbox service
                // resolves actual approval policy/route and returns only actionable items.
                var pendingResult = await _approvalInbox.GetPendingAsync(user, ct);
                response.PendingApprovals = pendingResult.IsSuccess
                    ? pendingResult.Data ?? new List<PendingApprovalGroupDto>()
                    : new List<PendingApprovalGroupDto>();

                return ServiceResult<DashboardResponse>.Ok(response);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DASHBOARD] Aggregation failed for UserId={UserId}", user.UserId);
                return ServiceResult<DashboardResponse>.Fail(
                    "Không thể tải Dashboard.");
            }
        }
    }
}

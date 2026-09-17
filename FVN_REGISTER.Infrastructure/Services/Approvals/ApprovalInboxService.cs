using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Application.Policies;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Approvals
{
    public class ApprovalInboxService : BaseService<ApprovalInboxService>, IApprovalInboxService
    {
        private readonly IApprovalWorkflowOrchestrator<LeaveRequestSubject> _leaveWorkflow;
        private readonly IApprovalWorkflowOrchestrator<OTRequestSubject> _otWorkflow;
        private readonly IApprovalGroupingPolicy _groupingPolicy;

        public ApprovalInboxService(
            IApprovalWorkflowOrchestrator<LeaveRequestSubject> leaveWorkflow,
            IApprovalWorkflowOrchestrator<OTRequestSubject> otWorkflow,
            IApprovalGroupingPolicy groupingPolicy,
            ILogger<ApprovalInboxService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _leaveWorkflow = leaveWorkflow;
            _otWorkflow = otWorkflow;
            _groupingPolicy = groupingPolicy;
        }

        public async Task<ServiceResult<List<PendingApprovalGroupDto>>> GetPendingAsync(
            UserIdentityDto user,
            CancellationToken ct = default)
        {
            try
            {
                var leaveItemsTask = _leaveWorkflow.GetPendingForApproverAsync(user.Email ?? "", ct);
                var otItemsTask = _otWorkflow.GetPendingForApproverAsync(user.Email ?? "", ct);
                await Task.WhenAll(leaveItemsTask, otItemsTask);

                var byModule = new Dictionary<RequestModule, List<PendingApprovalItemDto>>
                {
                    [RequestModule.Leave] = await leaveItemsTask,
                    [RequestModule.Overtime] = await otItemsTask
                };

                var grouped = _groupingPolicy.BuildGroups(byModule);
                return ServiceResult<List<PendingApprovalGroupDto>>.Ok(grouped);
            }
            catch (Exception ex)
            {
                return InternalError<List<PendingApprovalGroupDto>>(
                    ex, "Lỗi hệ thống khi tải danh sách chờ duyệt.");
            }
        }

        public async Task<ServiceResult> ApproveItemsAsync(
            List<int> ids,
            RequestModule kind,
            int level,
            string? comment,
            UserIdentityDto user,
            CancellationToken ct = default)
        {
            if (ids == null || ids.Count == 0)
                return ServiceResult.Fail("Không có đơn nào được chọn.");

            try
            {
                var action = BuildActionDto(ids, kind, level, comment, false, user);
                var result = kind switch
                {
                    RequestModule.Leave => await _leaveWorkflow.ApproveAsync(action, ct),
                    RequestModule.Overtime => await _otWorkflow.ApproveAsync(action, ct),
                    _ => throw new NotSupportedException($"Module {kind} chưa được hỗ trợ ở Inbox.")
                };

                return result.Success
                    ? ServiceResult.Ok(result.Message)
                    : ServiceResult.Fail(result.Message ?? "Duyệt thất bại.");
            }
            catch (NotSupportedException ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[INBOX] ApproveItems ERROR Kind={Kind}", kind);
                return ServiceResult.Fail("Lỗi hệ thống khi duyệt đơn.");
            }
        }

        public async Task<ServiceResult> RejectItemsAsync(
            List<int> ids,
            RequestModule kind,
            int level,
            string comment,
            UserIdentityDto user,
            CancellationToken ct = default)
        {
            if (ids == null || ids.Count == 0)
                return ServiceResult.Fail("Không có đơn nào được chọn.");
            if (string.IsNullOrWhiteSpace(comment))
                return ServiceResult.Fail("Lý do từ chối không được để trống.");

            try
            {
                var action = BuildActionDto(ids, kind, level, comment, true, user);
                var result = kind switch
                {
                    RequestModule.Leave => await _leaveWorkflow.RejectAsync(action, ct),
                    RequestModule.Overtime => await _otWorkflow.RejectAsync(action, ct),
                    _ => throw new NotSupportedException($"Module {kind} chưa được hỗ trợ ở Inbox.")
                };

                return result.Success
                    ? ServiceResult.Ok(result.Message)
                    : ServiceResult.Fail(result.Message ?? "Từ chối thất bại.");
            }
            catch (NotSupportedException ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[INBOX] RejectItems ERROR Kind={Kind}", kind);
                return ServiceResult.Fail("Lỗi hệ thống khi từ chối đơn.");
            }
        }

        private static ApprovalActionDto BuildActionDto(
            List<int> ids,
            RequestModule kind,
            int level,
            string? comment,
            bool isReject,
            UserIdentityDto user) => new()
            {
                RequestIds = ids,
                Kind = kind,
                Level = level,
                IsReject = isReject,
                Comment = comment,
                ApproverCode = user.EmployeeCode ?? "",
                ApproverPermission = user.Permission,
                ApproverPositionCode = user.PositionCode
            };
    }
}

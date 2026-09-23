
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Auths;

using FVN_REGISTER.Application.Policies;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Models.Subjects;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Approvals
{
    public class ApprovalInboxService : BaseService<ApprovalInboxService>, IApprovalInboxService
    {
        private readonly IApprovalWorkflowOrchestrator<LeaveRequestSubject> _leaveWorkflow;
        private readonly IApprovalWorkflowOrchestrator<OTRequestSubject> _otWorkflow;
        private readonly IApprovalWorkflowOrchestrator<TripRequestSubject> _tripWorkflow;
        private readonly IApprovalWorkflowOrchestrator<EquipmentRequestSubject> _equipmentWorkflow;
        private readonly IApprovalGroupingPolicy _groupingPolicy;
        private readonly IAuthorizationService _authorization;
        private readonly IApprovalPolicyService _approvalPolicies;
        private readonly IAuditService _audit;

        public ApprovalInboxService(
            IApprovalWorkflowOrchestrator<LeaveRequestSubject> leaveWorkflow,
            IApprovalWorkflowOrchestrator<OTRequestSubject> otWorkflow,
            IApprovalWorkflowOrchestrator<TripRequestSubject> tripWorkflow,
            IApprovalWorkflowOrchestrator<EquipmentRequestSubject> equipmentWorkflow,
            IApprovalGroupingPolicy groupingPolicy,
            IAuthorizationService authorization,
            IApprovalPolicyService approvalPolicies,
            IAuditService audit,
            ILogger<ApprovalInboxService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _leaveWorkflow = leaveWorkflow;
            _otWorkflow = otWorkflow;
            _tripWorkflow = tripWorkflow;
            _equipmentWorkflow = equipmentWorkflow;
            _groupingPolicy = groupingPolicy;
            _authorization = authorization;
            _approvalPolicies = approvalPolicies;
            _audit = audit;
        }

        public async Task<ServiceResult<List<PendingApprovalGroupDto>>> GetPendingAsync(
            UserIdentityDto user,
            CancellationToken ct = default)
        {
            try
            {
                // All approval workflows are scoped services and share the same UnitOfWork/DbContext.
                // Do not execute EF queries concurrently on those workflows: DbContext is not thread-safe.
                // Running these reads sequentially also keeps the current DI lifetime safe without
                // requiring a separate DbContextFactory for every workflow.
                var byModule = new Dictionary<RequestModule, List<PendingApprovalItemDto>>
                {
                    [RequestModule.Leave] = await _leaveWorkflow.GetPendingForApproverAsync(user.Email ?? "", ct),
                    [RequestModule.Overtime] = await _otWorkflow.GetPendingForApproverAsync(user.Email ?? "", ct),
                    [RequestModule.Trip] = await _tripWorkflow.GetPendingForApproverAsync(user.Email ?? "", ct),
                    [RequestModule.Equipment] = await _equipmentWorkflow.GetPendingForApproverAsync(user.Email ?? "", ct)
                };

                var scopedByModule = new Dictionary<RequestModule, List<PendingApprovalItemDto>>();
                foreach (var pair in byModule)
                {
                    var functionCode = pair.Key switch
                    {
                        RequestModule.Leave => SecurityFunctionCodes.LeaveApprove,
                        RequestModule.Overtime => SecurityFunctionCodes.OTApprove,
                        RequestModule.Trip => SecurityFunctionCodes.TripApprove,
                        RequestModule.Equipment => SecurityFunctionCodes.EquipmentApprove,
                        _ => 0
                    };

                    if (functionCode == 0)
                        continue;

                    var scopedItems = new List<PendingApprovalItemDto>();
                    foreach (var item in pair.Value)
                    {
                        var policyAllows = await _approvalPolicies.CanApproveAsync(
                            pair.Key,
                            item.EmployeeCode,
                            user.EmployeeCode ?? string.Empty,
                            GetCurrentLevel(item),
                            ct);

                        if (policyAllows && await _authorization.CanAccessAsync(
                            user,
                            functionCode,
                            item.EmployeeCode,
                            item.DeptCode,
                            ct))
                        {
                            scopedItems.Add(item);
                        }
                    }
                    scopedByModule[pair.Key] = scopedItems;
                }

                var grouped = _groupingPolicy.BuildGroups(scopedByModule);
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
                var validation = await ValidateRequestedItemsAsync(ids, kind, level, user, ct);
                if (!validation.Success)
                    return validation;

                var action = BuildActionDto(ids.Distinct().ToList(), kind, level, comment, false, user);
                var result = kind switch
                {
                    RequestModule.Leave => await _leaveWorkflow.ApproveAsync(action, ct),
                    RequestModule.Overtime => await _otWorkflow.ApproveAsync(action, ct),
                    RequestModule.Trip => await _tripWorkflow.ApproveAsync(action, ct),
                    RequestModule.Equipment => await _equipmentWorkflow.ApproveAsync(action, ct),
                    _ => throw new NotSupportedException($"Module {kind} chưa được hỗ trợ ở Inbox.")
                };

                if (result.Success)
                {
                    await _audit.LogAction("APPROVAL_APPROVED", user.UserId, $"Kind={kind}; Level={level}; RequestIds={string.Join(',', ids.Distinct())}", ct: ct);
                    return ServiceResult.Ok(result.Message);
                }
                return ServiceResult.Fail(result.Message ?? "Duyệt thất bại.");
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
                var validation = await ValidateRequestedItemsAsync(ids, kind, level, user, ct);
                if (!validation.Success)
                    return validation;

                var action = BuildActionDto(ids.Distinct().ToList(), kind, level, comment, true, user);
                var result = kind switch
                {
                    RequestModule.Leave => await _leaveWorkflow.RejectAsync(action, ct),
                    RequestModule.Overtime => await _otWorkflow.RejectAsync(action, ct),
                    RequestModule.Trip => await _tripWorkflow.RejectAsync(action, ct),
                    RequestModule.Equipment => await _equipmentWorkflow.RejectAsync(action, ct),
                    _ => throw new NotSupportedException($"Module {kind} chưa được hỗ trợ ở Inbox.")
                };

                if (result.Success)
                {
                    await _audit.LogAction("APPROVAL_REJECTED", user.UserId, $"Kind={kind}; Level={level}; RequestIds={string.Join(',', ids.Distinct())}", ct: ct);
                    return ServiceResult.Ok(result.Message);
                }
                return ServiceResult.Fail(result.Message ?? "Từ chối thất bại.");
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


        private async Task<ServiceResult> ValidateRequestedItemsAsync(
            List<int> ids,
            RequestModule kind,
            int level,
            UserIdentityDto user,
            CancellationToken ct)
        {
            var normalizedIds = ids.Distinct().Where(x => x > 0).ToList();
            if (normalizedIds.Count == 0)
                return ServiceResult.Fail("Không có đơn hợp lệ được chọn.");

            var pending = await GetPendingAsync(user, ct);
            if (!pending.Success || pending.Data == null)
                return ServiceResult.Fail(pending.Message ?? "Không thể xác thực phạm vi phê duyệt.");

            var allowed = pending.Data
                .SelectMany(x => x.Requests)
                .Where(x => x.Kind == kind && x.CanApprove && GetCurrentLevel(x) == level)
                .Select(x => x.RequestId)
                .ToHashSet();

            if (normalizedIds.Any(id => !allowed.Contains(id)))
                return ServiceResult.Fail("Một hoặc nhiều đơn không thuộc phạm vi phê duyệt của tài khoản hiện tại hoặc đã thay đổi trạng thái.");

            return ServiceResult.Ok();
        }

        private static int GetCurrentLevel(PendingApprovalItemDto item)
            => item.ApprovalSteps
                .Where(x => x.IsRequired && x.IsApproved == null)
                .OrderBy(x => x.Level)
                .Select(x => x.Level)
                .FirstOrDefault();

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

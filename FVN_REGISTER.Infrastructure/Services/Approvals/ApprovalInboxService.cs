using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Policies;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Utils;
using FVN_REGISTER.Infrastructure.Models.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;


namespace FVN_REGISTER.API.Services.Approvals
{
    /// <summary>
    /// Gộp pending list xuyên module bằng cách gọi trực tiếp 2 IApprovalWorkflowOrchestrator&lt;TSubject&gt;
    /// đã đăng ký cho Leave và OT. Vì các method trên interface đó không dùng TSubject trong chữ ký
    /// (chỉ để phân biệt DI registration), nên switch bằng RequestModule là đủ — không cần registry/factory.
    /// </summary>
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

        // ================= PENDING =================
        public async Task<ServiceResult<List<PendingApprovalGroupDto>>> GetPendingAsync(
            UserIdentityDto user,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[INBOX] GetPending: {Email}", user.Email);

                var leaveItemsTask = _leaveWorkflow.GetPendingForApproverAsync(user.Email ?? "", ct);
                var otItemsTask = _otWorkflow.GetPendingForApproverAsync(user.Email ?? "", ct);

                await Task.WhenAll(leaveItemsTask, otItemsTask);

                var byModule = new Dictionary<RequestModule, List<PendingApprovalItemDto>>
                {
                    [RequestModule.Leave] = await leaveItemsTask,
                    [RequestModule.Overtime] = await otItemsTask
                };

                // Đúng vai trò: Policy lo gộp/sắp xếp (CanApprove filter, RequestType, OverriddenCount,
                // sort theo TimeoutDays/SubmittedAt), Service chỉ gom dữ liệu thô từ các Workflow rồi giao việc.
                var grouped = _groupingPolicy.BuildGroups(byModule);

                var totalItems = byModule.Values.Sum(x => x.Count);
                Logger.LogInfoIf(Debug, "[INBOX] GetPending DONE: {Count} items, {GroupCount} groups", totalItems, grouped.Count);

                return ServiceResult<List<PendingApprovalGroupDto>>.Ok(grouped);
            }
            catch (Exception ex)
            {
                return InternalError<List<PendingApprovalGroupDto>>(
                    ex, "Lỗi hệ thống khi tải danh sách chờ duyệt.");
            }
        }

        // ================= APPROVE =================
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
                var action = BuildActionDto(ids, kind, level, comment, isReject: false, user);

                var result = kind switch
                {
                    RequestModule.Leave => await _leaveWorkflow.ApproveAsync(action, ct),
                    RequestModule.Overtime => await _otWorkflow.ApproveAsync(action, ct),
                    _ => throw new NotSupportedException($"Module {kind} chưa được hỗ trợ ở Inbox.")
                };

                Logger.LogInfoIf(Debug, "[INBOX] Approve {Kind} Level={Lv} Success={S}", kind, level, result.Success);

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

        // ================= REJECT =================
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
                var action = BuildActionDto(ids, kind, level, comment, isReject: true, user);

                var result = kind switch
                {
                    RequestModule.Leave => await _leaveWorkflow.RejectAsync(action, ct),
                    RequestModule.Overtime => await _otWorkflow.RejectAsync(action, ct),
                    _ => throw new NotSupportedException($"Module {kind} chưa được hỗ trợ ở Inbox.")
                };

                Logger.LogInfoIf(Debug, "[INBOX] Reject {Kind} Level={Lv} Success={S}", kind, level, result.Success);

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

        // ================= HELPER =================

        private static ApprovalActionDto BuildActionDto(
            List<int> ids, RequestModule kind, int level, string? comment, bool isReject, UserIdentityDto user) => new()
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

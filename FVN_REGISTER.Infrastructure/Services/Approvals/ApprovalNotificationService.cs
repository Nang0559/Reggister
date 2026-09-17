
using FVN_REGISTER.Application.Factories;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Rules;
using FVN_REGISTER.Application.Services.Common;

using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Core.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Infrastructure.Services.Approvals
{
    public class ApprovalNotificationService
      : BaseApplicationService<ApprovalNotificationService>, IApprovalNotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationFactory _factory;
        private readonly INotificationService _notification;
        private readonly IEmailService _email;
        private readonly IEmployeeUserResolver _userResolver;   // ★ MỚI

        public ApprovalNotificationService(
            IUnitOfWork uow,
            INotificationFactory factory,
            INotificationService notification,
            IEmailService email,
            IEmployeeUserResolver userResolver,                 // ★ MỚI
            ILogger<ApprovalNotificationService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
            _factory = factory;
            _notification = notification;
            _email = email;
            _userResolver = userResolver;                       // ★ MỚI
        }

        // ================= EMAIL: ĐƠN MỚI =================
        // (không đổi — dùng thẳng approverEmail, không cần resolve UserId)
        public async Task NotifyNewRequestAsync(string approverCode,
            string approverEmail, string approverName, int requestId,
            RequestModule requestType, string creatorName, int level,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(approverEmail))
            {
                Logger.LogWarnIf(Debug,
                    "[APPROVAL-NOTIFY] Bỏ qua email: approverEmail rỗng | RequestId={Id}", requestId);
                return;
            }

            var dto = _factory.CreateApprovalNotification(
                userId: 0,
                employeeCode: null,
                module: requestType,
                requestId: requestId,
                action: NotificationAction.Pending,
                level: level);

            var templateCode = $"{requestType.ToCode()}_REQUEST_NEW";

            await _email.QueueEmail(approverEmail, templateCode, new
            {
                ApproverName = approverName,
                CreatorName = creatorName,
                dto.Title,
                dto.Body,
                dto.ActionUrl
            }, ct);

            Logger.LogInfoIf(Debug,
                "[APPROVAL-NOTIFY] Queued email {Template} to {Email} | RequestId={Id}",
                templateCode, approverEmail, requestId);
        }

        // ================= IN-APP: APPROVER CÓ ĐƠN CHỜ =================
        public async Task NotifyApproverInAppAsync(
            string approverEmployeeCode, int requestId,
            RequestModule requestType, string creatorName, int level,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(approverEmployeeCode))
            {
                Logger.LogWarnIf(Debug,
                    "[APPROVAL-NOTIFY] Bỏ qua in-app: approverEmployeeCode rỗng | RequestId={Id}", requestId);
                return;
            }

            // ★ SỬA — dùng IEmployeeUserResolver thay cho ResolveUserIdAsync riêng
            var approverUserId = await _userResolver.ResolveUserIdAsync(approverEmployeeCode, ct);
            if (approverUserId is null or <= 0)
            {
                Logger.LogWarnIf(Debug,
                    "[APPROVAL-NOTIFY] Bỏ qua in-app: không tìm thấy UserId cho EmployeeCode={Code} | RequestId={Id}",
                    approverEmployeeCode, requestId);
                return;
            }

            var dto = _factory.CreateApprovalNotification(
                userId: approverUserId.Value,
                employeeCode: approverEmployeeCode,
                module: requestType,
                requestId: requestId,
                action: level > 1 ? NotificationAction.PendingNextLevel : NotificationAction.Pending,
                level: level);

            await _notification.CreateAsync(dto, ct);
        }

        // ================= IN-APP: NGƯỜI TẠO ĐƠN NHẬN KẾT QUẢ =================
        // (không đổi — đã có creatorUserId sẵn từ nơi gọi, không cần resolve)
        public async Task NotifyCreatorInAppAsync(
            int creatorUserId, string creatorEmployeeCode, string status,
            int requestId, RequestModule requestType,
            CancellationToken ct = default)
        {
            if (creatorUserId <= 0)
            {
                Logger.LogWarnIf(Debug,
                    "[APPROVAL-NOTIFY] Bỏ qua in-app: creatorUserId không hợp lệ | RequestId={Id}", requestId);
                return;
            }

            var action = NotificationActionRules.FromApprovalStatus(status);

            var dto = _factory.CreateApprovalNotification(
                userId: creatorUserId,
                employeeCode: creatorEmployeeCode,
                module: requestType,
                requestId: requestId,
                action: action);

            await _notification.CreateAsync(dto, ct);
        }

        // ================= IN-APP: LEO THANG (ESCALATE) =================
        // (không đổi — newApproverUserId đã có sẵn từ nơi gọi)
        public async Task NotifyEscalatedInAppAsync(
            int oldApproverUserId, int newApproverUserId, string newApproverEmployeeCode,
            int requestId, RequestModule requestType,
            CancellationToken ct = default)
        {
            if (newApproverUserId > 0)
            {
                var dto = _factory.CreateApprovalNotification(
                    userId: newApproverUserId,
                    employeeCode: newApproverEmployeeCode,
                    module: requestType,
                    requestId: requestId,
                    action: NotificationAction.Escalated);

                await _notification.CreateAsync(dto, ct);
            }

            Logger.LogInfoIf(Debug,
                "[APPROVAL-NOTIFY] Escalated RequestId={Id} | {Old} -> {New}",
                requestId, oldApproverUserId, newApproverUserId);
        }

        // ================= ❌ ĐÃ XÓA: SendPendingRemindersAsync =================
        // Lý do: đọc F03ApprovalStep (bảng cũ, Obsolete) + field ReminderSent
        // (KHÔNG tồn tại ở kiến trúc mới). Reminder giờ do
        // ApprovalEscalationService<TSubject>.ProcessStepAsync tự xử lý hoàn
        // toàn — tự ghi F03ApprovalReminderLog, tự gửi email — KHÔNG đi qua
        // ApprovalNotificationService nữa. Giữ lại method này sẽ gây gửi email
        // nhắc trùng lặp (double reminder) nếu còn nơi nào gọi nhầm.

        // ================= ❌ ĐÃ XÓA: private ResolveUserIdAsync =================
        // Thay bằng IEmployeeUserResolver (constructor-injected), tránh mỗi
        // service tự viết lại query resolve UserId từ EmployeeCode.
    }
}
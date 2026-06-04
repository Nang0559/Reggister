using FVN_REGISTER.Contract.Interfaces.Emails;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Services.OT
{
    public class OTNotificationService
        : BaseService<OTNotificationService>, IOTNotificationService
    {
        private readonly IEmailService _email;

        public OTNotificationService(
            IEmailService email,
            ILogger<OTNotificationService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _email = email;
        }

        // ================= 1. GỬI YÊU CẦU DUYỆT =================
        public async Task SendApprovalRequestAsync(
            string approverEmail,
            F03OTRequest otRequest,
            string requesterName,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(approverEmail)) return;

            Logger.LogDebugIf(Debug,
                "[OT_NOTIFY] SendApprovalRequest to {Email} for OT {Id}",
                approverEmail, otRequest.Id);

            await _email.QueueEmail(approverEmail, "OT_REQUEST_NEW", new
            {
                RequesterName = requesterName,
                OTDate = otRequest.OTDate.ToString("dd/MM/yyyy"),
                StartTime = otRequest.StartTime.ToString(@"hh\:mm"),
                EndTime = otRequest.EndTime.ToString(@"hh\:mm"),
                TotalHours = otRequest.TotalOTHours,
                Reason = otRequest.Reason,
                OTType = otRequest.OTTypeCode,
                ApproveUrl = $"https://yourdomain/ot/approve/{otRequest.Id}"
            }, ct);
        }

        // ================= 2. THÔNG BÁO THAY ĐỔI TRẠNG THÁI =================
        public async Task SendStatusChangedAsync(
            F03OTRequest otRequest,
            string newStatus,
            CancellationToken ct = default)
        {
            // Lấy email người tạo đơn từ danh sách nhân viên OT
            // Ưu tiên gửi cho người tạo đơn
            var targetEmail = otRequest.CreatedByEmail;
            if (string.IsNullOrWhiteSpace(targetEmail)) return;

            Logger.LogDebugIf(Debug,
                "[OT_NOTIFY] StatusChanged OT {Id} → {Status}",
                otRequest.Id, newStatus);

            string template = newStatus switch
            {
                "Approved" => "OT_APPROVED",
                "Rejected" => "OT_REJECTED",
                "Cancelled" => "OT_CANCELLED",
                _ => "OT_STATUS_CHANGED"
            };

            await _email.QueueEmail(targetEmail, template, new
            {
                OTDate = otRequest.OTDate.ToString("dd/MM/yyyy"),
                TotalHours = otRequest.TotalOTHours,
                NewStatus = newStatus,
                StatusDisplay = GetStatusDisplay(newStatus),
                DateNotify = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                DetailUrl = $"https://yourdomain/ot/detail/{otRequest.Id}"
            }, ct);
        }

        // ================= 3. NHẮC NHỞ APPROVER CHƯA DUYỆT =================
        public async Task SendReminderAsync(
            F03OTRequest otRequest,
            int level,
            CancellationToken ct = default)
        {
            string? targetEmail = level switch
            {
                3 => otRequest.Level3ApproveEmail,
                5 => otRequest.Level5ApproveEmail,
                6 => otRequest.Level6ApproveEmail,
                7 => otRequest.Level7ApproveEmail,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(targetEmail)) return;

            Logger.LogDebugIf(Debug,
                "[OT_NOTIFY] Reminder OT {Id} Level {Level}",
                otRequest.Id, level);

            await _email.QueueEmail(targetEmail, "OT_REMINDER", new
            {
                OTId = otRequest.Id,
                OTDate = otRequest.OTDate.ToString("dd/MM/yyyy"),
                TotalHours = otRequest.TotalOTHours,
                Level = level,
                ApproveUrl = $"https://yourdomain/ot/approve/{otRequest.Id}"
            }, ct);
        }

        private static string GetStatusDisplay(string status) => status switch
        {
            "Approved" => "Đã phê duyệt",
            "Rejected" => "Đã từ chối",
            "Cancelled" => "Đã hủy",
            "Pending" => "Chờ duyệt",
            _ => status
        };
    }
}
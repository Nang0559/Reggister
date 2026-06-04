using FVN_REGISTER.Contract.Interfaces.Emails;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Services.OT
{
    public class OTNotificationService
        : BaseService<OTNotificationService>, IOTNotificationService
    {
        private readonly IEmailService _email;
        private readonly FVNWEBAPPContext _db;

        public OTNotificationService(
            IEmailService email,
            FVNWEBAPPContext db,
            ILogger<OTNotificationService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _email = email;
            _db = db;
        }

        // ════════════════════════════════════════════════════════════════════
        // GỬI YÊU CẦU PHÊ DUYỆT TỚI APPROVER CẤP TIẾP THEO
        // ════════════════════════════════════════════════════════════════════
        public async Task SendApprovalRequestAsync(
            string approverEmail,
            string approverName,
            F03OTRequest otRequest,
            string creatorName,
            int level,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(approverEmail)) return;

            Logger.LogDebugIf(Debug,
                "[OT_NOTIFY] Send approval request Level={Level} → {Email}",
                level, approverEmail);

            var levelName = level switch
            {
                3 => "Sub-leader / Leader",
                5 => "Ast. Chief / Chief",
                6 => "A.MG / MG",
                7 => "GM",
                _ => $"Level {level}"
            };

            await _email.QueueEmail(approverEmail, "OT_REQUEST_NEW", new
            {
                ApproverName = approverName,
                LevelName = levelName,
                CreatorName = creatorName,
                OTCode = otRequest.OTCode,
                OTDate = otRequest.OTDate.ToString("dd/MM/yyyy"),
                StartTime = otRequest.StartTime.ToString(@"hh\:mm"),
                EndTime = otRequest.EndTime.ToString(@"hh\:mm"),
                TotalHours = otRequest.TotalOTHours,
                OTType = OTTypeConst.GetDisplayName(otRequest.OTTypeCode),
                Reason = otRequest.OTReason,
                Url = $"https://yourdomain/ot/details/{otRequest.Id}"
            }, ct);
        }

        // ════════════════════════════════════════════════════════════════════
        // THÔNG BÁO KẾT QUẢ (APPROVED / REJECTED) CHO NGƯỜI TẠO ĐƠN
        // ════════════════════════════════════════════════════════════════════
        public async Task SendStatusChangedAsync(
            string targetEmail,
            string targetName,
            string status,
            F03OTRequest otRequest,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(targetEmail)) return;

            Logger.LogDebugIf(Debug,
                "[OT_NOTIFY] Status changed {Status} → {Email}",
                status, targetEmail);

            var templateCode = status == OTStatus.Approved
                ? "OT_APPROVED"
                : "OT_REJECTED";

            await _email.QueueEmail(targetEmail, templateCode, new
            {
                EmployeeName = targetName,
                OTCode = otRequest.OTCode,
                OTDate = otRequest.OTDate.ToString("dd/MM/yyyy"),
                TotalHours = otRequest.TotalOTHours,
                Status = OTStatus.GetDisplayName(status),
                DateNotify = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            }, ct);
        }

        // ════════════════════════════════════════════════════════════════════
        // QUÉT VÀ GỬI NHẮC NHỞ CÁC ĐƠN QUÁ HẠN CHƯA ĐƯỢC DUYỆT
        // ════════════════════════════════════════════════════════════════════
        public async Task SendPendingRemindersAsync(CancellationToken ct = default)
        {
            Logger.LogDebugIf(Debug, "[OT_NOTIFY] Start pending reminder scan");

            var pendingRequests = await _db.F03OTRequests
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && OTStatus.ActiveStatuses.Contains(x.RequestStatus))
                .ToListAsync(ct);

            int sent = 0;
            foreach (var req in pendingRequests)
            {
                ct.ThrowIfCancellationRequested();

                // Xác định email approver đang chờ duyệt ở bước hiện tại
                string? targetEmail = req.RequestStatus switch
                {
                    OTStatus.Pending => req.Level3IsApprove == null
                                                ? req.Level3ApproveEmail
                                                : req.Level5ApproveEmail,
                    OTStatus.ApprovedLv3 => req.Level5ApproveEmail,
                    OTStatus.ApprovedLv5 => req.Level6ApproveEmail,
                    OTStatus.ApprovedLv6 => req.Level7ApproveEmail,
                    _ => null
                };

                if (string.IsNullOrWhiteSpace(targetEmail)) continue;

                await _email.QueueEmail(targetEmail, "OT_REMINDER", new
                {
                    OTId = req.Id,
                    OTCode = req.OTCode,
                    OTDate = req.OTDate.ToString("dd/MM/yyyy"),
                    TotalHours = req.TotalOTHours,
                    Status = OTStatus.GetDisplayName(req.RequestStatus),
                    Url = $"https://yourdomain/ot/details/{req.Id}"
                }, ct);

                sent++;
            }

            Logger.LogInfoIf(Debug,
                "[OT_NOTIFY] Reminder scan done: {Sent}/{Total}",
                sent, pendingRequests.Count);
        }
    }
}
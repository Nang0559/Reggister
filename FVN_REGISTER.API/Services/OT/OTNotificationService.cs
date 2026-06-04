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
    public class OTNotificationService : BaseService<OTNotificationService>, IOTNotificationService
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

        // ── Gửi yêu cầu duyệt cho approver hiện tại ───────────────
        public async Task SendApprovalRequestAsync(
            string approverEmail,
            F03OTRequest request,
            string requesterName,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(approverEmail)) return;

            try
            {
                Logger.LogDebugIf(Debug,
                    "[OT-MAIL] SendApprovalRequest to {Email} for {Code}",
                    approverEmail, request.OTCode);

                await _email.QueueEmail(approverEmail, "OT_REQUEST_NEW", new
                {
                    RequesterName = requesterName,
                    OTCode = request.OTCode,
                    OTDate = request.OTDate.ToString("dd/MM/yyyy"),
                    StartTime = request.StartTime.ToString(@"hh\:mm"),
                    EndTime = request.EndTime.ToString(@"hh\:mm"),
                    PlannedHours = request.PlannedHours,
                    OTType = OTTypeConst.GetDisplayName(request.OTTypeCode),
                    Reason = request.OTReason,
                    Url = $"https://yourdomain/ot/approvals"
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT-MAIL] SendApprovalRequest ERROR for {Code}", request.OTCode);
            }
        }

        // ── Thông báo kết quả (Approved / Rejected) cho người tạo ─
        public async Task SendStatusChangedAsync(
            F03OTRequest request,
            string newStatus,
            CancellationToken ct = default)
        {
            // Lấy email người tạo đơn
            var creatorEmail = await _db.F03employees
                .AsNoTracking()
                .Where(x => x.EmployeeCode == request.EmployeeCode && x.IsActive)
                .Select(x => x.EmailAddress)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(creatorEmail)
                && !string.IsNullOrWhiteSpace(request.CreatedByEmail))
                creatorEmail = request.CreatedByEmail;

            if (string.IsNullOrWhiteSpace(creatorEmail)) return;

            try
            {
                Logger.LogDebugIf(Debug,
                    "[OT-MAIL] StatusChanged {Status} → {Email} for {Code}",
                    newStatus, creatorEmail, request.OTCode);

                string templateCode = newStatus == OTStatus.Approved
                    ? "OT_APPROVED"
                    : "OT_REJECTED";

                await _email.QueueEmail(creatorEmail, templateCode, new
                {
                    OTCode = request.OTCode,
                    OTDate = request.OTDate.ToString("dd/MM/yyyy"),
                    Status = OTStatus.GetDisplayName(newStatus),
                    DateNotify = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    Url = $"https://yourdomain/ot/history"
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT-MAIL] SendStatusChanged ERROR for {Code}", request.OTCode);
            }
        }

        // ── Nhắc nhở approver chưa duyệt (Background job) ─────────
        public async Task SendReminderAsync(CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[OT-MAIL] Reminder scan start");

                var pendingList = await _db.F03OTRequests
                    .AsNoTracking()
                    .Where(x => x.IsActive == true
                             && OTStatus.ActiveStatuses.Contains(x.RequestStatus))
                    .ToListAsync(ct);

                foreach (var req in pendingList)
                {
                    ct.ThrowIfCancellationRequested();

                    // Xác định approver hiện tại cần nhắc
                    string? targetEmail = null;
                    string? targetName = null;

                    bool hasLv3 = !string.IsNullOrEmpty(req.Level3ApproveEmail);

                    if (hasLv3 && req.Level3IsApprove == null)
                    {
                        targetEmail = req.Level3ApproveEmail;
                        targetName = req.Level3ApproveName;
                    }
                    else if (req.Level5IsApprove == null
                          && (req.Level3IsApprove == true || !hasLv3))
                    {
                        targetEmail = req.Level5ApproveEmail;
                        targetName = req.Level5ApproveName;
                    }
                    else if (req.Level5IsApprove == true && req.Level6IsApprove == null)
                    {
                        targetEmail = req.Level6ApproveEmail;
                        targetName = req.Level6ApproveName;
                    }
                    else if (req.Level6IsApprove == true && req.Level7IsApprove == null
                          && !string.IsNullOrEmpty(req.Level7ApproveEmail))
                    {
                        targetEmail = req.Level7ApproveEmail;
                        targetName = req.Level7ApproveName;
                    }

                    if (string.IsNullOrWhiteSpace(targetEmail)) continue;

                    await _email.QueueEmail(targetEmail, "OT_REMINDER", new
                    {
                        ApproverName = targetName ?? "Approver",
                        OTCode = req.OTCode,
                        OTDate = req.OTDate.ToString("dd/MM/yyyy"),
                        Url = "https://yourdomain/ot/approvals"
                    }, ct);
                }

                Logger.LogInfoIf(Debug,
                    "[OT-MAIL] Reminder done: {Count} đơn chờ duyệt",
                    pendingList.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT-MAIL] Reminder ERROR");
            }
        }
    }
}
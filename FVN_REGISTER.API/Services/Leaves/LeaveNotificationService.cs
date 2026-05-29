using FVN_REGISTER.Contract.Interfaces.Emails;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FVN_REGISTER.API.Services.Leaves
{
    public class LeaveNotificationService
      : BaseService<LeaveNotificationService>, ILeaveNotificationService
    {
        private readonly IEmailService _email;
        private readonly FVNWEBAPPContext _db;

        public LeaveNotificationService(
            IEmailService email,
            FVNWEBAPPContext db,
            ILogger<LeaveNotificationService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _email = email;
            _db = db;
        }

        // ================= 1. GỬI YÊU CẦU DUYỆT =================
        public async Task SendApprovalRequestAsync(
            string approverEmail,
            string approverName,
            F03leaveDay leave,
            string employeeName,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(approverEmail)) return;

            Logger.LogDebugIf(Debug, "[MAIL] Send approval request to {Email}", approverEmail);

            await _email.QueueEmail(approverEmail, "LEAVE_REQUEST_NEW", new
            {
                ApproverName = approverName,
                EmployeeName = employeeName,
                StartDate = leave.StartDate.ToString("dd/MM/yyyy"),
                EndDate = leave.EndDate.ToString("dd/MM/yyyy"),
                TotalDay = leave.TotalDay,
                Reason = leave.LeaveReason,
                Url = $"https://yourdomain/Leave/Details/{leave.Id}"
            });
        }

        // ================= 2. THÔNG BÁO KẾT QUẢ =================
        public async Task SendStatusChangedNotificationAsync(
            string targetEmail,
            string targetName,
            string status,
            string employeeCode,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(targetEmail)) return;

            Logger.LogDebugIf(Debug, "[MAIL] Status notify {Email} - {Status}", targetEmail, status);

            string template = status == "Approved"
                ? "LEAVE_APPROVED"
                : "LEAVE_REJECTED";

            await _email.QueueEmail(targetEmail, template, new
            {
                EmployeeName = targetName,
                EmployeeCode = employeeCode,
                Status = status,
                DateNotify = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            });
        }

        // ================= 3. AUTO REMINDER =================
        public async Task SendApprovalNotificationAsync(CancellationToken ct = default)
        {
            Logger.LogDebugIf(Debug, "[MAIL] Start reminder scan");

            var pendingLeaves = await _db.F03leaveDays
                .AsNoTracking()
                .Where(x => x.IsActive == true && x.RequestStatus == "Pending")
                .ToListAsync(ct);

            foreach (var leave in pendingLeaves)
            {
                ct.ThrowIfCancellationRequested();

                string? targetEmail = leave.Level1IsApprove == null
                    ? leave.Level1ApproveEmail
                    : leave.Level2IsApprove == null
                        ? leave.Level2ApproveEmail
                        : leave.Level3ApproveEmail;

                if (!string.IsNullOrWhiteSpace(targetEmail))
                {
                    await _email.QueueEmail(targetEmail, "LEAVE_REMINDER", new
                    {
                        LeaveId = leave.Id,
                        EmployeeCode = leave.EmployeeCode
                    });
                }
            }

            Logger.LogInfoIf(Debug, "[MAIL] Reminder scan done: {Count}", pendingLeaves.Count);
        }
    }
}

using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Interfaces.Auths;
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
        private readonly INotificationService _notification;
        public LeaveNotificationService(
            IEmailService email,
            FVNWEBAPPContext db,
            INotificationService notification,
            ILogger<LeaveNotificationService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _email = email;
            _db = db;
            _notification = notification;
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
            // Lấy UserId của approver để tạo in-app notification
            var approverUser = await _db.F03users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmployeeCode != null &&
                    _db.F03employees.Any(e => e.EmployeeCode == u.EmployeeCode
                                           && e.EmailAddress == approverEmail), ct);

            if (approverUser != null)
            {
                await _notification.CreateAsync(new CreateNotificationDto
                {
                    UserId = approverUser.IdUser,
                    EmployeeCode = approverUser.EmployeeCode, // Nếu cần lưu mã nhân viên người nhận
                    NotificationType = "LEAVE_PENDING",       // Đúng tên trường 'NotificationType'
                    Title = "Đơn nghỉ phép cần duyệt",
                    Body = $"{employeeName} xin nghỉ {leave.TotalDay} ngày từ {leave.StartDate:dd/MM}",
                    RelatedLeaveId = leave.Id                  // Đúng tên trường 'RelatedLeaveId' (kiểu int?)
                }, ct);
            }
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
            var targetUser = await _db.F03users
       .AsNoTracking()
       .FirstOrDefaultAsync(u => u.EmployeeCode != null &&
           _db.F03employees.Any(e => e.EmployeeCode == u.EmployeeCode
                                  && e.EmailAddress == targetEmail), ct);

            if (targetUser != null)
            {
                var titleMap = status == "Approved" ? "Đơn nghỉ đã được duyệt" : "Đơn nghỉ bị từ chối";

                await _notification.CreateAsync(new CreateNotificationDto
                {
                    UserId = targetUser.IdUser,
                    EmployeeCode = targetUser.EmployeeCode,
                    Title = titleMap,
                    // 🌟 SỬA 'DateNotify' thành DateTime.Now nếu không có biến ngày sẵn dùng
                    Body = $"Trạng thái: {status} — Vào lúc {DateTime.Now:dd/MM/yyyy HH:mm}",
                    NotificationType = status == "Approved" ? "LEAVE_APPROVED" : "LEAVE_REJECTED",

                    // 🌟 SỬA 'leave.Id': Thay bằng biến Id đơn nghỉ thực tế trong hàm của bạn (Ví dụ: leaveId, model.Id,...)
                    // Nếu không có, hãy để là: RelatedLeaveId = null
                    RelatedLeaveId = null
                }, ct);
            }
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

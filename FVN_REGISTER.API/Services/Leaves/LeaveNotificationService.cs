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
    
        public class LeaveNotificationService : BaseService<LeaveNotificationService>, ILeaveNotificationService
        {
            private readonly IEmailService _email;
            private readonly INotificationService _sysNotification;
            private readonly FVNWEBAPPContext _db;

            public LeaveNotificationService(
                IEmailService email,
                INotificationService sysNotification,
                FVNWEBAPPContext db,
                ILogger<LeaveNotificationService> logger,
                IOptionsMonitor<AuthDebugOptions> options) : base(logger, options)
            {
                _email = email;
                _sysNotification = sysNotification;
                _db = db;
            }

            // ================= NGHIỆP VỤ 1: KHI CÓ ĐƠN NGHỈ PHÉP MỚI =================
            public async Task NotifyNewLeaveRequestAsync(
                string approverEmail,
                int leaveId,
                string employeeName,
                CancellationToken cancellationToken)
            {
                // 1. Logic Gửi EMAIL - Truy vấn thực thể trực tiếp bằng Id từ DB chạy ngầm
                var leaveForEmail = await _db.F03leaveDays
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == leaveId, cancellationToken);

                if (leaveForEmail != null)
                {
                    // ✅ Sửa lỗi: Đổi từ _emailSvc thành _email theo biến đã inject ở Constructor
                    await _email.SendApprovalRequestAsync(
                        approverEmail,
                        "Approver",
                        leaveForEmail,
                        employeeName,
                        cancellationToken);
                }
                else
                {
                    Logger.LogWarning("[LEAVE-NOTIFY] Cannot send email. Leave ID {LeaveId} not found.", leaveId);
                }

                // 2. Logic Bắn IN-APP thông báo hệ thống (SignalR / Database Notification)
                var approverUser = await _db.F03users.AsNoTracking()
                    .Join(_db.F03employees,
                        u => u.EmployeeCode,
                        e => e.EmployeeCode,
                        (u, e) => new { u.IdUser, e.EmailAddress })
                    .FirstOrDefaultAsync(x => x.EmailAddress == approverEmail, cancellationToken);

                if (approverUser != null)
                {
                    // ✅ Cách 1: Sử dụng cấu trúc CreateAsync như nghiệp vụ thay đổi trạng thái của bạn
                    await _sysNotification.CreateAsync(new CreateNotificationDto
                    {
                        UserId = approverUser.IdUser,
                        NotificationType = "LEAVE_PENDING",
                        Title = "Đơn nghỉ phép mới",
                        Body = $"{employeeName} vừa gửi đơn nghỉ phép cần phê duyệt",
                        ActionUrl = $"/approve/list?id={leaveId}"
                    }, cancellationToken);

                    /* // 💡 Cách 2: Nếu interface INotificationService của bạn dùng hàm đặc định giống như code cũ:
                    await _sysNotification.NotifyPendingLeaveAsync(
                        approverUserId: approverUser.IdUser,
                        employeeName: employeeName,
                        leaveId: leaveId,
                        ct: cancellationToken);
                    */

                    Logger.LogDebugIf(Debug, "[LEAVE-NOTIFY] In-app notification pushed to Approver UserID: {UserId}", approverUser.IdUser);
                }
                else
                {
                    Logger.LogWarning("[LEAVE-NOTIFY] Approver user not found in F03users for email: {Email}", approverEmail);
                }
            }

            // ================= NGHIỆP VỤ 2: KHI TRẠNG THÁI ĐƠN THAY ĐỔI =================
            public async Task NotifyStatusChangedAsync(
                int requesterUserId,
                string targetEmail,
                string targetName,
                string status,
                string employeeCode,
                int leaveId,
                CancellationToken ct = default)
            {
                // 1. Gửi Kênh Email
                if (!string.IsNullOrWhiteSpace(targetEmail))
                {
                    string emailTemplate = status == "Approved" ? "LEAVE_APPROVED" : "LEAVE_REJECTED";
                    // Đảm bảo hàm QueueEmail hoặc SendEmail của _email nhận đúng cấu trúc object này của bạn
                    await _email.QueueEmail(targetEmail, emailTemplate, new { EmployeeName = targetName, Status = status, LeaveId = leaveId }, ct);
                }

                // 2. Gửi Kênh App Notification dựa trên trạng thái phê duyệt mới
                var (type, title) = status switch
                {
                    "Approved" => ("LEAVE_APPROVED", "Đơn nghỉ phép của bạn đã được duyệt ✓"),
                    "Rejected" => ("LEAVE_REJECTED", "Đơn nghỉ phép của bạn bị từ chối ✗"),
                    _ => ("SYSTEM", "Trạng thái đơn nghỉ đã thay đổi")
                };

                await _sysNotification.CreateAsync(new CreateNotificationDto
                {
                    UserId = requesterUserId,
                    NotificationType = type,
                    Title = title,
                    Body = $"Đơn nghỉ phép của bạn đã chuyển sang trạng thái: {status}",
                    ActionUrl = "/leave/history",
                    RelatedLeaveId = leaveId // Sửa/Thêm nếu CreateNotificationDto của bạn map trường này
                }, ct);
            }
        }

    
}

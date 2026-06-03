using FVN_REGISTER.Contract.Interfaces.Emails;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace FVN_REGISTER.API.Services.Emails
{
   

    public class EmailService : BaseService<EmailService>, IEmailService
    {
        private readonly FVNWEBAPPContext _db;

        public EmailService(
            FVNWEBAPPContext db,
            ILogger<EmailService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
        }

        // ================= ĐỒNG BỘ NGHIỆP VỤ: GỬI YÊU CẦU DUYỆT ĐƠN NGHỈ PHÉP =================
        public async Task SendApprovalRequestAsync(
            string toEmail,
            string recipientRole,
            F03leaveDay leave,
            string employeeName,
            CancellationToken ct = default)
        {
            try
            {
                // Định nghĩa dữ liệu truyền vào mẫu template email
                var emailPayload = new
                {
                    EmployeeName = employeeName,
                    Role = recipientRole,
                    StartDate = leave.StartDate.ToString("dd/MM/yyyy") ?? "",
                    EndDate = leave.EndDate.ToString("dd/MM/yyyy") ?? "",
                    TotalDay = leave.TotalDay,
                    Reason = leave.LeaveReason ?? "",
                    LeaveId = leave.Id
                };

                // Tiến hành đẩy vào hàng đợi gửi Email ngầm (Tránh nghẽn kênh SMTP chính)
                await QueueEmail(toEmail, "LEAVE_APPROVAL_REQUEST", emailPayload, ct);

                Logger.LogDebugIf(Debug, "[EMAIL] Dispatched leave approval email request to queue for: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[EMAIL] SendApprovalRequestAsync failed for LeaveId={LeaveId}", leave.Id);
                // Không throw để đảm bảo luồng chạy ngầm của NotifyNewLeaveRequestAsync không bị crash đột ngột
            }
        }

        // ================= QUEUE EMAIL =================
        public async Task QueueEmail(
            string to,
            string templateCode,
            object data,
            CancellationToken ct = default)
        {
            try
            {
                var queue = new EmailQueue
                {
                    ToEmail = to,
                    TemplateCode = templateCode,
                    Payload = JsonConvert.SerializeObject(data),
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    RetryCount = 0,
                    MaxRetry = 3
                };

                _db.EmailQueues.Add(queue);
                await _db.SaveChangesAsync(ct);

                Logger.LogDebugIf(Debug,
                    "[EMAIL] Queued to {To} | Template {Template}",
                    to, templateCode);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[EMAIL] QueueEmail error");
                throw;
            }
        }

        // ================= PROCESS QUEUE =================
        public async Task ProcessQueue(CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[EMAIL] Processing queue start");

                var emails = await _db.EmailQueues
                    .Where(x => x.Status == "Pending" || x.Status == "Retry")
                    .OrderBy(x => x.CreatedAt)
                    .Take(50)
                    .ToListAsync(ct);

                foreach (var email in emails)
                {
                    ct.ThrowIfCancellationRequested();

                    try
                    {
                        var template = await _db.EmailTemplates
                            .FirstOrDefaultAsync(x =>
                                x.Code == email.TemplateCode &&
                                x.IsActive == true, ct);

                        if (template == null)
                        {
                            MarkFailed(email, $"Template '{email.TemplateCode}' not found or inactive.");
                            continue;
                        }

                        string subject = RenderTemplate(template.Subject, email.Payload);
                        string body = RenderTemplate(template.Body, email.Payload);

                        await SendEmail(email.ToEmail, subject, body, ct);

                        email.Status = "Sent";
                        email.SentAt = DateTime.Now;
                        email.ErrorMessage = null;
                    }
                    catch (Exception ex)
                    {
                        HandleRetry(email, ex.Message);
                        Logger.LogWarnIf(Debug,
                            "[EMAIL] Send failed {Id}: {Error}",
                            email.Id, ex.Message);
                    }
                }

                await _db.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[EMAIL] Queue processed: {Count}", emails.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[EMAIL] ProcessQueue error");
                throw;
            }
        }

        // ================= SEND EMAIL DIRECT (SMTP) =================
        public async Task SendEmail(
            string to,
            string subject,
            string body,
            CancellationToken ct = default)
        {
            var profile = await _db.F03emailProfiles
                .Where(x => x.Code == "ITSYS" && x.IsActive == true)
                .OrderByDescending(x => x.ModifiedAt)
                .FirstOrDefaultAsync(ct);

            if (profile == null)
                throw new Exception("Active Email profile 'ITSYS' not found in database.");

            using var smtp = new SmtpClient
            {
                Host = profile.EmailServerName,
                Port = profile.EmailServerPort,
                EnableSsl = profile.EmailServerEnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    profile.EmailAccountName,
                    EncryptUtils.MD5Decrypt(profile.EmailPassword)
                )
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(profile.EmailAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            await smtp.SendMailAsync(mail, ct);

            Logger.LogDebugIf(Debug, "[EMAIL] Sent to {To}", to);
        }

        // ================= TEMPLATE RENDER =================
        public string RenderTemplate(string template, string jsonPayload)
        {
            if (string.IsNullOrEmpty(template) || string.IsNullOrEmpty(jsonPayload))
                return template ?? "";

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonPayload);
            if (dict == null) return template;

            foreach (var item in dict)
            {
                template = template.Replace($"{{{{{item.Key}}}}}", item.Value?.ToString() ?? "");
            }

            return template;
        }

        // ================= RETRY =================
        public async Task RetryFailedEmails(CancellationToken ct = default)
        {
            var emails = await _db.EmailQueues
                .Where(x => x.Status == "Retry" && x.RetryCount < x.MaxRetry)
                .ToListAsync(ct);

            foreach (var e in emails)
            {
                e.Status = "Pending";
            }

            await _db.SaveChangesAsync(ct);

            Logger.LogInfoIf(Debug, "[EMAIL] Retry reset: {Count}", emails.Count);
        }

        // ================= QUERIES =================
        public async Task<int> GetPendingCount(CancellationToken ct = default) =>
            await _db.EmailQueues.CountAsync(x => x.Status == "Pending", ct);

        public async Task<int> GetFailedCount(CancellationToken ct = default) =>
            await _db.EmailQueues.CountAsync(x => x.Status == "Failed", ct);

        public async Task<List<EmailQueue>> GetFailedEmails(CancellationToken ct = default) =>
            await _db.EmailQueues
                .Where(x => x.Status == "Failed")
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

        public async Task CancelEmail(int id, CancellationToken ct = default)
        {
            var email = await _db.EmailQueues.FindAsync(new object[] { id }, ct);
            if (email == null || email.Status == "Sent") return;

            email.Status = "Cancelled";
            await _db.SaveChangesAsync(ct);
        }

        public async Task ResendEmail(int id, CancellationToken ct = default)
        {
            var email = await _db.EmailQueues.FindAsync(new object[] { id }, ct);
            if (email == null) return;

            email.Status = "Pending";
            email.RetryCount = 0;

            await _db.SaveChangesAsync(ct);
        }

        // ================= HELPERS =================
        private void HandleRetry(EmailQueue email, string error)
        {
            email.RetryCount++;
            email.ErrorMessage = error;
            email.Status = email.RetryCount >= email.MaxRetry ? "Failed" : "Retry";
        }

        private void MarkFailed(EmailQueue email, string error)
        {
            email.Status = "Failed";
            email.ErrorMessage = error;
        }
    }
}

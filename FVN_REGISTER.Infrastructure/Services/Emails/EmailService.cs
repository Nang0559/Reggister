
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace FVN_REGISTER.Infrastructure.Services.Emails
{
    public class EmailService : BaseService<EmailService>, IEmailService
    {
        private readonly IUnitOfWork _uow;

        public EmailService(
            IUnitOfWork uow,
            ILogger<EmailService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
        }

        // ================= ĐỒNG BỘ NGHIỆP VỤ: GỬI YÊU CẦU DUYỆT ĐƠN NGHỈ PHÉP =================
        public async Task SendApprovalRequestAsync(
            string toEmail,
            string recipientRole,
            F03LeaveDay leave,
            string employeeName,
            CancellationToken ct = default)
        {
            try
            {
                var emailPayload = new
                {
                    EmployeeName = employeeName,
                    Role = recipientRole,
                    StartDate = leave.StartDate.ToString("dd/MM/yyyy"),
                    EndDate = leave.EndDate.ToString("dd/MM/yyyy"),
                    TotalDay = leave.TotalDay,
                    Reason = leave.LeaveReason ?? "",
                    LeaveId = leave.Id
                };

                await QueueEmail(toEmail, "LEAVE_APPROVAL_REQUEST", emailPayload, ct);

                Logger.LogDebugIf(Debug, "[EMAIL] Dispatched leave approval email request to queue for: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[EMAIL] SendApprovalRequestAsync failed for LeaveId={LeaveId}", leave.Id);
                // Không throw để không làm crash luồng chạy ngầm gọi hàm này
            }
        }

        // ================= QUEUE EMAIL =================
        public async Task<EmailQueueResult> QueueEmail(
            string to,
            string templateCode,
            object data,
            CancellationToken ct = default)
        {
            try
            {
                var templateExists = await _uow.Repository<F03EmailTemplate>().Query()
                    .AnyAsync(x => x.Code == templateCode && x.IsActive == true, ct);

                if (!templateExists)
                {
                    Logger.LogWarning(
                        "[EMAIL] ⚠️ Template '{TemplateCode}' chưa khai báo. Email tới [{To}] KHÔNG được queue.",
                        templateCode, to);

                    return EmailQueueResult.TemplateNotFound(templateCode);
                }

                var queue = new F03EmailQueue
                {
                    ToEmail = to,
                    TemplateCode = templateCode,
                    Payload = JsonConvert.SerializeObject(data),
                    Status = EmailStatus.Pending,
                    RetryCount = 0,
                    MaxRetry = 3
                };

                await _uow.Repository<F03EmailQueue>().AddAsync(queue, ct);
                await _uow.SaveChangesAsync(ct);

                Logger.LogDebugIf(Debug, "[EMAIL] Queued to {To} | Template {Template}", to, templateCode);

                return EmailQueueResult.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[EMAIL] QueueEmail error");
                return EmailQueueResult.Fail($"Lỗi hệ thống khi queue email: {ex.Message}");
            }
        }

        // ================= PROCESS QUEUE =================
        // Nhặt cả Pending (lần đầu) lẫn Retry (đã fail, còn lượt thử) trong 1 lượt chạy worker
        public async Task ProcessQueue(CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[EMAIL] Processing queue start");

                var repo = _uow.Repository<F03EmailQueue>();

                var emails = await repo.Query()
                    .Where(x => x.Status == EmailStatus.Pending || x.Status == EmailStatus.Retry)
                    .OrderBy(x => x.CreatedAt)
                    .Take(50)
                    .ToListAsync(ct);

                foreach (var email in emails)
                {
                    ct.ThrowIfCancellationRequested();

                    try
                    {
                        var template = await _uow.Repository<F03EmailTemplate>().Query()
                            .FirstOrDefaultAsync(x =>
                                x.Code == email.TemplateCode &&
                                x.IsActive == true, ct);

                        if (template == null)
                        {
                            Logger.LogWarning(
                                "[EMAIL] ⚠️ Template '{Code}' không tồn tại hoặc chưa kích hoạt. Email Id={Id} To={To} bị FAILED.",
                                email.TemplateCode, email.Id, email.ToEmail);

                            MarkFailed(email, $"Template '{email.TemplateCode}' chưa được khai báo hoặc chưa kích hoạt.");
                            continue;
                        }

                        string subject = RenderTemplate(template.Subject, email.Payload ?? "");
                        string body = RenderTemplate(template.Body, email.Payload ?? "");

                        await SendEmail(email.ToEmail, subject, body, ct);

                        email.Status = EmailStatus.Sent;
                        email.SentAt = DateTime.Now;
                        email.ErrorMessage = null;

                        Logger.LogDebugIf(Debug, "[EMAIL] Sent Id={Id} To={To} Template={Code}",
                            email.Id, email.ToEmail, email.TemplateCode);
                    }
                    catch (Exception ex)
                    {
                        HandleRetry(email, ex.Message);
                        Logger.LogWarnIf(Debug, "[EMAIL] Send failed Id={Id} To={To}: {Error}",
                            email.Id, email.ToEmail, ex.Message);
                    }
                }

                await _uow.SaveChangesAsync(ct);

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
            var profile = await _uow.Repository<F03EmailProfile>().Query()
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

        // ================= RETRY (thủ công, do Admin bấm) =================
        public async Task RetryFailedEmails(CancellationToken ct = default)
        {
            var repo = _uow.Repository<F03EmailQueue>();

            var emails = await repo.Query()
                .Where(x => x.Status == EmailStatus.Failed && x.RetryCount < x.MaxRetry)
                .ToListAsync(ct);

            foreach (var e in emails)
            {
                e.Status = EmailStatus.Retry;
            }

            await _uow.SaveChangesAsync(ct);

            Logger.LogInfoIf(Debug, "[EMAIL] Retry reset: {Count}", emails.Count);
        }

        // ================= QUERIES =================
        public async Task<int> GetPendingCount(CancellationToken ct = default) =>
            await _uow.Repository<F03EmailQueue>().Query()
                .CountAsync(x => x.Status == EmailStatus.Pending, ct);

        public async Task<int> GetFailedCount(CancellationToken ct = default) =>
            await _uow.Repository<F03EmailQueue>().Query()
                .CountAsync(x => x.Status == EmailStatus.Failed, ct);

        public async Task<List<F03EmailQueue>> GetFailedEmails(CancellationToken ct = default) =>
            await _uow.Repository<F03EmailQueue>().Query()
                .Where(x => x.Status == EmailStatus.Failed)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

        public async Task CancelEmail(int id, CancellationToken ct = default)
        {
            var repo = _uow.Repository<F03EmailQueue>();
            var email = await repo.GetByIdAsync(id, ct);
            if (email == null || email.Status == EmailStatus.Sent) return;

            email.Status = EmailStatus.Cancelled;

            await _uow.SaveChangesAsync(ct);
        }

        public async Task ResendEmail(int id, CancellationToken ct = default)
        {
            var repo = _uow.Repository<F03EmailQueue>();
            var email = await repo.GetByIdAsync(id, ct);
            if (email == null) return;

            email.Status = EmailStatus.Pending;
            email.RetryCount = 0;
            email.ErrorMessage = null;

            await _uow.SaveChangesAsync(ct);
        }

        // ================= HELPERS =================
        private void HandleRetry(F03EmailQueue email, string error)
        {
            email.RetryCount++;
            email.ErrorMessage = error;
            email.Status = email.RetryCount >= email.MaxRetry
                ? EmailStatus.Failed
                : EmailStatus.Retry;
        }

        private void MarkFailed(F03EmailQueue email, string error)
        {
            email.Status = EmailStatus.Failed;
            email.ErrorMessage = error;
        }
    }
}

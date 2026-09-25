using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using FVN_REGISTER.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Text.Json;

namespace FVN_REGISTER.Infrastructure.Services.Emails;

public class EmailService : BaseService<EmailService>, IEmailService
{
    private readonly IUnitOfWork _uow;

    public EmailService(IUnitOfWork uow, ILogger<EmailService> logger, IOptionsMonitor<AuthDebugOptions> options)
        : base(logger, options) => _uow = uow;

    public async Task SendApprovalRequestAsync(LeaveApprovalEmailDto data, string toEmail, CancellationToken ct = default)
    {
        var result = await QueueEmail(toEmail, "LEAVE_APPROVAL_REQUEST", new
        {
            data.EmployeeName,
            Role = data.RecipientRole,
            StartDate = data.StartDate.ToString("dd/MM/yyyy"),
            EndDate = data.EndDate.ToString("dd/MM/yyyy"),
            data.TotalDay,
            data.Reason,
            data.LeaveId
        }, ct);

        if (!result.IsSuccess)
            Logger.LogWarning("[EMAIL] Leave approval notification not dispatched: {Message}", result.Message);
    }

    public async Task<EmailQueueResult> QueueEmail(string to, string templateCode, object data, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(to)) return EmailQueueResult.Fail("Email người nhận không được để trống.");
        if (string.IsNullOrWhiteSpace(templateCode)) return EmailQueueResult.Fail("TemplateCode không được để trống.");

        try
        {
            var template = await _uow.Repository<F03EmailTemplate>().Query()
                .FirstOrDefaultAsync(x => x.Code == templateCode && x.IsActive == true, ct);
            if (template == null) return EmailQueueResult.TemplateNotFound(templateCode);

            var payload = JsonSerializer.Serialize(data);
            var subject = RenderTemplate(template.Subject, payload);
            var body = RenderTemplate(template.Body, payload);
            ValidateRenderedTemplate(templateCode, subject, body);

            var policy = await _uow.Repository<F03EmailDispatchPolicy>().Query()
                .Where(x => x.TemplateCode == templateCode && x.IsActive == true)
                .OrderBy(x => x.Priority)
                .FirstOrDefaultAsync(ct);

            var mode = policy?.DispatchMode ?? EmailDispatchMode.AutoSend;
            if (policy == null)
                Logger.LogWarning("[EMAIL] No dispatch policy for {TemplateCode}; compatibility default AutoSend is used.", templateCode);

            if (mode == EmailDispatchMode.Disabled)
                return EmailQueueResult.Skipped($"Email template '{templateCode}' đang được cấu hình tắt gửi.");

            var profileCode = policy?.EmailProfileCode;
            if (string.IsNullOrWhiteSpace(profileCode))
            {
                profileCode = await _uow.Repository<F03EmailProfile>().Query()
                    .Where(x => x.IsActive == true && x.IsDefault)
                    .Select(x => x.Code)
                    .FirstOrDefaultAsync(ct);
            }
            if (string.IsNullOrWhiteSpace(profileCode))
                return EmailQueueResult.Fail("Chưa có Email Profile active/default để gửi email.");

            var queue = new F03EmailQueue
            {
                ToEmail = to.Trim(),
                Subject = subject,
                Body = body,
                TemplateCode = templateCode,
                EmailProfileCode = profileCode,
                DispatchMode = mode,
                Payload = JsonSerializer.Serialize(data),
                Status = mode == EmailDispatchMode.QueueForApproval ? EmailStatus.WaitingApproval : EmailStatus.Pending,
                RetryCount = 0,
                MaxRetry = 3,
                CreatedAt = DateTime.Now
            };

            await _uow.Repository<F03EmailQueue>().AddAsync(queue, ct);
            await _uow.SaveChangesAsync(ct);
            return EmailQueueResult.Ok(mode == EmailDispatchMode.QueueForApproval ? "Email đã được đưa vào hàng đợi chờ duyệt." : "Email đã được đưa vào hàng đợi gửi tự động.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[EMAIL] QueueEmail error for {TemplateCode}", templateCode);
            return EmailQueueResult.Fail("Lỗi hệ thống khi tạo email queue.");
        }
    }

    public async Task ProcessQueue(CancellationToken ct = default)
    {
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
                email.Status = EmailStatus.Processing;
                email.LastAttemptAt = DateTime.Now;
                await _uow.SaveChangesAsync(ct);

                var profileCode = email.EmailProfileCode;
                if (string.IsNullOrWhiteSpace(profileCode))
                    profileCode = await _uow.Repository<F03EmailProfile>().Query()
                        .Where(x => x.IsActive == true && x.IsDefault)
                        .Select(x => x.Code).FirstOrDefaultAsync(ct);

                var profile = await _uow.Repository<F03EmailProfile>().Query()
                    .FirstOrDefaultAsync(x => x.Code == profileCode && x.IsActive == true, ct);
                if (profile == null)
                {
                    MarkFailed(email, $"Email Profile '{profileCode}' chưa được khai báo hoặc chưa active.");
                    continue;
                }

                await SendUsingProfile(email.ToEmail, email.Subject, email.Body, profile, ct);
                email.Status = EmailStatus.Sent;
                email.SentAt = DateTime.Now;
                email.ErrorMessage = null;

                await _uow.Repository<F03EmailLog>().AddAsync(new F03EmailLog
                {
                    QueueId = email.Id,
                    ToEmail = email.ToEmail,
                    Subject = email.Subject,
                    Status = EmailStatus.Sent,
                    SentAt = DateTime.Now
                }, ct);
            }
            catch (Exception ex)
            {
                HandleRetry(email, ex.Message);
                await _uow.Repository<F03EmailLog>().AddAsync(new F03EmailLog
                {
                    QueueId = email.Id,
                    ToEmail = email.ToEmail,
                    Subject = email.Subject,
                    Status = email.Status,
                    SentAt = DateTime.Now,
                    ErrorMessage = ex.Message
                }, ct);
            }
        }

        await _uow.SaveChangesAsync(ct);
    }

    public async Task SendEmail(string to, string subject, string body, CancellationToken ct = default)
    {
        var profile = await _uow.Repository<F03EmailProfile>().Query()
            .Where(x => x.IsActive == true && x.IsDefault)
            .OrderByDescending(x => x.ModifiedAt)
            .FirstOrDefaultAsync(ct);
        if (profile == null) throw new InvalidOperationException("Chưa cấu hình Email Profile mặc định.");
        await SendUsingProfile(to, subject, body, profile, ct);
    }

    private static async Task SendUsingProfile(string to, string subject, string body, F03EmailProfile profile, CancellationToken ct)
    {
        using var smtp = EmailAdminService.BuildSmtp(profile);
        using var mail = new MailMessage
        {
            From = new MailAddress(profile.EmailAddress, profile.FromName ?? "FCC Smart Portal"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        if (!string.IsNullOrWhiteSpace(profile.ReplyTo)) mail.ReplyToList.Add(profile.ReplyTo);
        mail.To.Add(to);
        await smtp.SendMailAsync(mail, ct);
    }

    public string RenderTemplate(string template, string jsonPayload)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;
        if (string.IsNullOrEmpty(jsonPayload)) return template;
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonPayload);
        if (dict == null) return template;
        foreach (var item in dict)
        {
            var value = item.Value.ValueKind switch
            {
                JsonValueKind.Null => string.Empty,
                JsonValueKind.String => item.Value.GetString() ?? string.Empty,
                _ => item.Value.ToString()
            };
            template = template.Replace($"{{{{{item.Key}}}}}", value, StringComparison.Ordinal);
        }
        return template;
    }

    private static void ValidateRenderedTemplate(string code, string subject, string body)
    {
        if (subject.Contains("{{", StringComparison.Ordinal) || subject.Contains("}}", StringComparison.Ordinal) ||
            body.Contains("{{", StringComparison.Ordinal) || body.Contains("}}", StringComparison.Ordinal))
            throw new InvalidOperationException($"Template '{code}' còn biến chưa được cung cấp.");
    }

    public async Task RetryEmailBatchAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return;
        var emails = await _uow.Repository<F03EmailQueue>().Query().Where(x => ids.Contains(x.Id)).ToListAsync(ct);
        foreach (var email in emails)
        {
            if (email.Status == EmailStatus.Sent) continue;
            email.Status = EmailStatus.Pending;
            email.RetryCount = 0;
            email.ErrorMessage = null;
        }
        await _uow.SaveChangesAsync(ct);
    }

    public async Task CancelEmailBatchAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return;
        var emails = await _uow.Repository<F03EmailQueue>().Query().Where(x => ids.Contains(x.Id) && x.Status != EmailStatus.Sent).ToListAsync(ct);
        foreach (var email in emails) { email.Status = EmailStatus.Cancelled; email.CancelledAt = DateTime.Now; }
        await _uow.SaveChangesAsync(ct);
    }

    public async Task CancelEmail(int id, CancellationToken ct = default)
    {
        var email = await _uow.Repository<F03EmailQueue>().GetByIdAsync(id, ct);
        if (email == null || email.Status == EmailStatus.Sent) return;
        email.Status = EmailStatus.Cancelled;
        email.CancelledAt = DateTime.Now;
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ApproveEmail(int id, int userId, CancellationToken ct = default)
    {
        var email = await _uow.Repository<F03EmailQueue>().GetByIdAsync(id, ct);
        if (email == null) throw new KeyNotFoundException("Không tìm thấy email trong queue.");
        if (email.Status != EmailStatus.WaitingApproval) throw new InvalidOperationException("Email không ở trạng thái chờ duyệt.");
        email.Status = EmailStatus.Pending;
        email.ApprovedBy = userId;
        email.ApprovedAt = DateTime.Now;
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ResendEmail(int id, CancellationToken ct = default)
    {
        var email = await _uow.Repository<F03EmailQueue>().GetByIdAsync(id, ct);
        if (email == null) return;
        if (email.Status == EmailStatus.WaitingApproval) return;
        email.Status = EmailStatus.Pending;
        email.RetryCount = 0;
        email.ErrorMessage = null;
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<List<EmailQueueDto>> GetQueueAsync(int take = 500, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        return await _uow.Repository<F03EmailQueue>().Query().AsNoTracking()
            .OrderByDescending(x => x.CreatedAt).Take(take)
            .Select(x => new EmailQueueDto
            {
                Id = x.Id, ToEmail = x.ToEmail, Subject = x.Subject, Body = x.Body,
                TemplateCode = x.TemplateCode, EmailProfileCode = x.EmailProfileCode,
                DispatchMode = x.DispatchMode.ToString(), Payload = x.Payload,
                Status = x.Status.ToString(), RetryCount = x.RetryCount, MaxRetry = x.MaxRetry,
                ErrorMessage = x.ErrorMessage, CreatedAt = x.CreatedAt,
                LastAttemptAt = x.LastAttemptAt, ApprovedAt = x.ApprovedAt, SentAt = x.SentAt
            }).ToListAsync(ct);
    }

    private static void HandleRetry(F03EmailQueue email, string error)
    {
        email.RetryCount++;
        email.ErrorMessage = error;
        email.Status = email.RetryCount >= email.MaxRetry ? EmailStatus.Failed : EmailStatus.Retry;
    }

    private static void MarkFailed(F03EmailQueue email, string error)
    {
        email.Status = EmailStatus.Failed;
        email.ErrorMessage = error;
    }
}

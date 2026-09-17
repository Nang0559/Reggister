using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using FVN_REGISTER.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace FVN_REGISTER.Infrastructure.Services.Emails;

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

    public async Task SendApprovalRequestAsync(
        LeaveApprovalEmailDto data,
        string toEmail,
        CancellationToken ct = default)
    {
        try
        {
            await QueueEmail(toEmail, "LEAVE_APPROVAL_REQUEST", new
            {
                data.EmployeeName,
                Role = data.RecipientRole,
                StartDate = data.StartDate.ToString("dd/MM/yyyy"),
                EndDate = data.EndDate.ToString("dd/MM/yyyy"),
                data.TotalDay,
                data.Reason,
                data.LeaveId
            }, ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "[EMAIL] SendApprovalRequestAsync failed for LeaveId={LeaveId}", data.LeaveId);
        }
    }

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
                return EmailQueueResult.TemplateNotFound(templateCode);

            var queue = new F03EmailQueue
            {
                ToEmail = to,
                TemplateCode = templateCode,
                Payload = JsonSerializer.Serialize(data),
                Status = EmailStatus.Pending,
                RetryCount = 0,
                MaxRetry = 3
            };

            await _uow.Repository<F03EmailQueue>().AddAsync(queue, ct);
            await _uow.SaveChangesAsync(ct);
            return EmailQueueResult.Ok();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[EMAIL] QueueEmail error");
            return EmailQueueResult.Fail($"Lỗi hệ thống khi queue email: {ex.Message}");
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
                var template = await _uow.Repository<F03EmailTemplate>().Query()
                    .FirstOrDefaultAsync(x => x.Code == email.TemplateCode && x.IsActive == true, ct);
                if (template == null)
                {
                    MarkFailed(email, $"Template '{email.TemplateCode}' chưa được khai báo hoặc chưa kích hoạt.");
                    continue;
                }

                var subject = RenderTemplate(template.Subject, email.Payload ?? "");
                var body = RenderTemplate(template.Body, email.Payload ?? "");
                await SendEmail(email.ToEmail, subject, body, ct);
                email.Status = EmailStatus.Sent;
                email.SentAt = DateTime.Now;
                email.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                HandleRetry(email, ex.Message);
            }
        }

        await _uow.SaveChangesAsync(ct);
    }

    public async Task SendEmail(string to, string subject, string body, CancellationToken ct = default)
    {
        var profile = await _uow.Repository<F03EmailProfile>().Query()
            .Where(x => x.Code == "ITSYS" && x.IsActive == true)
            .OrderByDescending(x => x.ModifiedAt)
            .FirstOrDefaultAsync(ct);

        if (profile == null)
            throw new InvalidOperationException("Active Email profile 'ITSYS' not found in database.");

        using var smtp = new SmtpClient
        {
            Host = profile.EmailServerName,
            Port = profile.EmailServerPort,
            EnableSsl = profile.EmailServerEnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(profile.EmailAccountName, EncryptUtils.MD5Decrypt(profile.EmailPassword))
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
    }

    public string RenderTemplate(string template, string jsonPayload)
    {
        if (string.IsNullOrEmpty(template) || string.IsNullOrEmpty(jsonPayload))
            return template ?? "";

        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonPayload);
        if (dict == null) return template;

        foreach (var item in dict)
        {
            var value = item.Value.ValueKind switch
            {
                JsonValueKind.Null => "",
                JsonValueKind.String => item.Value.GetString() ?? "",
                _ => item.Value.ToString()
            };
            template = template.Replace($"{{{{{item.Key}}}}}", value ?? "");
        }

        return template;
    }

    public async Task RetryFailedEmails(CancellationToken ct = default)
    {
        var emails = await _uow.Repository<F03EmailQueue>().Query()
            .Where(x => x.Status == EmailStatus.Failed && x.RetryCount < x.MaxRetry)
            .ToListAsync(ct);
        foreach (var email in emails) email.Status = EmailStatus.Retry;
        await _uow.SaveChangesAsync(ct);
    }

    public Task<int> GetPendingCount(CancellationToken ct = default) =>
        _uow.Repository<F03EmailQueue>().Query().CountAsync(x => x.Status == EmailStatus.Pending, ct);

    public Task<int> GetFailedCount(CancellationToken ct = default) =>
        _uow.Repository<F03EmailQueue>().Query().CountAsync(x => x.Status == EmailStatus.Failed, ct);

    public async Task<List<EmailQueueDto>> GetFailedEmails(CancellationToken ct = default)
        => await ProjectQueue(_uow.Repository<F03EmailQueue>().Query().Where(x => x.Status == EmailStatus.Failed))
            .OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public async Task<List<EmailQueueDto>> GetQueueAsync(int take = 500, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        return await ProjectQueue(_uow.Repository<F03EmailQueue>().Query())
            .OrderByDescending(x => x.CreatedAt).Take(take).ToListAsync(ct);
    }

    public async Task RetryEmailBatchAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return;
        var emails = await _uow.Repository<F03EmailQueue>().Query().Where(x => ids.Contains(x.Id)).ToListAsync(ct);
        foreach (var email in emails)
        {
            email.Status = EmailStatus.Pending;
            email.RetryCount = 0;
            email.ErrorMessage = null;
        }
        await _uow.SaveChangesAsync(ct);
    }

    public async Task CancelEmailBatchAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return;
        var emails = await _uow.Repository<F03EmailQueue>().Query()
            .Where(x => ids.Contains(x.Id) && x.Status != EmailStatus.Sent).ToListAsync(ct);
        foreach (var email in emails) email.Status = EmailStatus.Cancelled;
        await _uow.SaveChangesAsync(ct);
    }

    public async Task CancelEmail(int id, CancellationToken ct = default)
    {
        var email = await _uow.Repository<F03EmailQueue>().GetByIdAsync(id, ct);
        if (email == null || email.Status == EmailStatus.Sent) return;
        email.Status = EmailStatus.Cancelled;
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ResendEmail(int id, CancellationToken ct = default)
    {
        var email = await _uow.Repository<F03EmailQueue>().GetByIdAsync(id, ct);
        if (email == null) return;
        email.Status = EmailStatus.Pending;
        email.RetryCount = 0;
        email.ErrorMessage = null;
        await _uow.SaveChangesAsync(ct);
    }

    private static IQueryable<EmailQueueDto> ProjectQueue(IQueryable<F03EmailQueue> query) =>
        query.Select(x => new EmailQueueDto
        {
            Id = x.Id,
            ToEmail = x.ToEmail,
            TemplateCode = x.TemplateCode,
            Payload = x.Payload,
            Status = x.Status.ToString(),
            RetryCount = x.RetryCount,
            MaxRetry = x.MaxRetry,
            ErrorMessage = x.ErrorMessage,
            CreatedAt = x.CreatedAt,
            SentAt = x.SentAt
        });

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

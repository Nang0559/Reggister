using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace FVN_REGISTER.Infrastructure.Services.Emails;

public sealed class EmailAdminService : BaseService<EmailAdminService>, IEmailAdminService
{
    private readonly IUnitOfWork _uow;

    public EmailAdminService(
        IUnitOfWork uow,
        ILogger<EmailAdminService> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(logger, options)
    {
        _uow = uow;
    }

    public async Task<List<EmailProfileDto>> GetProfilesAsync(CancellationToken ct = default) =>
        await _uow.Repository<F03EmailProfile>().Query().AsNoTracking()
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Code)
            .Select(x => ToDto(x)).ToListAsync(ct);

    public async Task<EmailProfileDto?> GetProfileAsync(int id, CancellationToken ct = default)
    {
        var x = await _uow.Repository<F03EmailProfile>().GetByIdAsync(id, ct);
        return x == null ? null : ToDto(x);
    }

    public async Task<ServiceResult> SaveProfileAsync(EmailProfileDto dto, int userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Code)) return ServiceResult.Fail("Mã Email Profile không được để trống.");
        if (string.IsNullOrWhiteSpace(dto.EmailServerName)) return ServiceResult.Fail("SMTP Server không được để trống.");
        if (dto.EmailServerPort <= 0 || dto.EmailServerPort > 65535) return ServiceResult.Fail("SMTP Port không hợp lệ.");
        if (string.IsNullOrWhiteSpace(dto.EmailAddress)) return ServiceResult.Fail("Email From không được để trống.");

        var repo = _uow.Repository<F03EmailProfile>();
        var duplicate = await repo.Query().AnyAsync(x => x.Code == dto.Code.Trim() && x.Id != dto.Id, ct);
        if (duplicate) return ServiceResult.Fail($"Email Profile '{dto.Code}' đã tồn tại.");

        F03EmailProfile entity;
        if (dto.Id == 0)
        {
            entity = new F03EmailProfile { Code = dto.Code.Trim().ToUpperInvariant(), CreatedBy = userId, CreatedAt = DateTime.Now };
            await repo.AddAsync(entity, ct);
        }
        else
        {
            entity = await repo.GetByIdAsync(dto.Id, ct) ?? throw new KeyNotFoundException("Không tìm thấy Email Profile.");
            entity.ModifiedBy = userId;
            entity.ModifiedAt = DateTime.Now;
        }

        entity.Name = dto.Name.Trim();
        entity.EmailServerName = dto.EmailServerName.Trim();
        entity.EmailServerType = string.IsNullOrWhiteSpace(dto.EmailServerType) ? "SMTP" : dto.EmailServerType.Trim().ToUpperInvariant();
        entity.EmailServerPort = dto.EmailServerPort;
        entity.EmailServerEnableSsl = dto.EmailServerEnableSsl;
        entity.SecurityMode = string.IsNullOrWhiteSpace(dto.SecurityMode) ? "STARTTLS" : dto.SecurityMode.Trim().ToUpperInvariant();
        entity.AuthenticationType = string.IsNullOrWhiteSpace(dto.AuthenticationType) ? "Basic" : dto.AuthenticationType.Trim();
        entity.EmailAccountName = dto.EmailAccountName.Trim();
        entity.EmailAddress = dto.EmailAddress.Trim();
        entity.FromName = dto.FromName?.Trim();
        entity.ReplyTo = dto.ReplyTo?.Trim();
        entity.SiteUrl = dto.SiteUrl?.Trim();
        entity.IsActive = dto.IsActive;
        entity.TimeoutSeconds = Math.Clamp(dto.TimeoutSeconds, 5, 300);
        if (!string.IsNullOrWhiteSpace(dto.Password)) entity.EmailPassword = EncryptUtils.MD5Encrypt(dto.Password);

        if (dto.IsDefault)
        {
            var others = await repo.Query().Where(x => x.Id != entity.Id && x.IsDefault).ToListAsync(ct);
            foreach (var other in others) other.IsDefault = false;
            entity.IsDefault = true;
        }

        await _uow.SaveChangesAsync(ct);
        return ServiceResult.Ok("Đã lưu Email Profile.");
    }

    public async Task<ServiceResult> TestProfileAsync(int id, string toEmail, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail)) return ServiceResult.Fail("Email nhận thử không được để trống.");
        var profile = await _uow.Repository<F03EmailProfile>().GetByIdAsync(id, ct);
        if (profile == null) return ServiceResult.Fail("Không tìm thấy Email Profile.");

        try
        {
            using var smtp = BuildSmtp(profile);
            using var mail = new MailMessage
            {
                From = new MailAddress(profile.EmailAddress, profile.FromName ?? "FCC Smart Portal"),
                Subject = "[FCC Smart Portal] SMTP Test",
                Body = $"<p>SMTP profile <strong>{WebUtility.HtmlEncode(profile.Code)}</strong> đã gửi thử thành công.</p><p>Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>",
                IsBodyHtml = true
            };
            if (!string.IsNullOrWhiteSpace(profile.ReplyTo)) mail.ReplyToList.Add(profile.ReplyTo);
            mail.To.Add(toEmail.Trim());
            await smtp.SendMailAsync(mail, ct);
            return ServiceResult.Ok("Kết nối SMTP và gửi email kiểm tra thành công.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[EMAIL_PROFILE] SMTP test failed for {Code}", profile.Code);
            return ServiceResult.Fail($"SMTP test thất bại: {ex.Message}");
        }
    }

    public async Task<List<EmailDispatchPolicyDto>> GetPoliciesAsync(CancellationToken ct = default) =>
        await _uow.Repository<F03EmailDispatchPolicy>().Query().AsNoTracking()
            .OrderBy(x => x.TemplateCode).ThenBy(x => x.Priority)
            .Select(x => new EmailDispatchPolicyDto
            {
                Id = x.Id,
                TemplateCode = x.TemplateCode,
                EmailProfileCode = x.EmailProfileCode,
                DispatchMode = x.DispatchMode.ToString(),
                Priority = x.Priority,
                IsActive = x.IsActive == true,
                Description = x.Description
            }).ToListAsync(ct);

    public async Task<ServiceResult> SavePolicyAsync(EmailDispatchPolicyDto dto, int userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.TemplateCode)) return ServiceResult.Fail("TemplateCode không được để trống.");
        if (!Enum.TryParse<EmailDispatchMode>(dto.DispatchMode, true, out var mode)) return ServiceResult.Fail("DispatchMode không hợp lệ.");
        var templateExists = await _uow.Repository<F03EmailTemplate>().Query().AnyAsync(x => x.Code == dto.TemplateCode.Trim() && x.IsActive == true, ct);
        if (!templateExists) return ServiceResult.Fail($"Template '{dto.TemplateCode}' chưa tồn tại hoặc chưa active.");
        var profileExists = await _uow.Repository<F03EmailProfile>().Query().AnyAsync(x => x.Code == dto.EmailProfileCode.Trim() && x.IsActive == true, ct);
        if (!profileExists) return ServiceResult.Fail($"Email Profile '{dto.EmailProfileCode}' chưa tồn tại hoặc chưa active.");

        var repo = _uow.Repository<F03EmailDispatchPolicy>();
        var entity = dto.Id == 0
            ? new F03EmailDispatchPolicy { CreatedBy = userId, CreatedAt = DateTime.Now }
            : await repo.GetByIdAsync(dto.Id, ct) ?? throw new KeyNotFoundException("Không tìm thấy Email Dispatch Policy.");
        if (dto.Id == 0) await repo.AddAsync(entity, ct);
        else { entity.ModifiedBy = userId; entity.ModifiedAt = DateTime.Now; }

        entity.TemplateCode = dto.TemplateCode.Trim().ToUpperInvariant();
        entity.EmailProfileCode = dto.EmailProfileCode.Trim().ToUpperInvariant();
        entity.DispatchMode = mode;
        entity.Priority = dto.Priority;
        entity.IsActive = dto.IsActive;
        entity.Description = dto.Description?.Trim();
        await _uow.SaveChangesAsync(ct);
        return ServiceResult.Ok("Đã lưu chính sách gửi email.");
    }

    public async Task<ServiceResult> TogglePolicyAsync(int id, int userId, CancellationToken ct = default)
    {
        var entity = await _uow.Repository<F03EmailDispatchPolicy>().GetByIdAsync(id, ct);
        if (entity == null) return ServiceResult.Fail("Không tìm thấy chính sách.");
        entity.IsActive = !entity.IsActive;
        entity.ModifiedBy = userId;
        entity.ModifiedAt = DateTime.Now;
        await _uow.SaveChangesAsync(ct);
        return ServiceResult.Ok(entity.IsActive == true ? "Đã kích hoạt chính sách." : "Đã vô hiệu hóa chính sách.");
    }

    private static EmailProfileDto ToDto(F03EmailProfile x) => new()
    {
        Id = x.Id, Code = x.Code, Name = x.Name, EmailServerName = x.EmailServerName,
        EmailServerType = x.EmailServerType, EmailServerPort = x.EmailServerPort,
        EmailServerEnableSsl = x.EmailServerEnableSsl, SecurityMode = x.SecurityMode,
        AuthenticationType = x.AuthenticationType, EmailAccountName = x.EmailAccountName,
        EmailAddress = x.EmailAddress, FromName = x.FromName, ReplyTo = x.ReplyTo,
        SiteUrl = x.SiteUrl, IsDefault = x.IsDefault, IsActive = x.IsActive == true,
        TimeoutSeconds = x.TimeoutSeconds, HasPassword = !string.IsNullOrWhiteSpace(x.EmailPassword)
    };

    internal static SmtpClient BuildSmtp(F03EmailProfile profile)
    {
        var smtp = new SmtpClient
        {
            Host = profile.EmailServerName,
            Port = profile.EmailServerPort,
            EnableSsl = profile.EmailServerEnableSsl,
            UseDefaultCredentials = false,
            Timeout = Math.Clamp(profile.TimeoutSeconds, 5, 300) * 1000
        };
        if (string.Equals(profile.AuthenticationType, "None", StringComparison.OrdinalIgnoreCase))
            smtp.UseDefaultCredentials = true;
        else
            smtp.Credentials = new NetworkCredential(profile.EmailAccountName, string.IsNullOrWhiteSpace(profile.EmailPassword) ? string.Empty : EncryptUtils.MD5Decrypt(profile.EmailPassword));
        return smtp;
    }
}

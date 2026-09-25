using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/email-admin")]
public sealed class EmailAdminController : BaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly IAuthorizationService _authorization;

    public EmailAdminController(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        IAuthorizationService authorization,
        ILogger<EmailAdminController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options)
    {
        _uow = uow;
        _authorization = authorization;
    }

    [HttpGet("profiles")]
    public async Task<IActionResult> Profiles(CancellationToken ct)
    {
        if (!await CanManage(ct)) return Forbid();
        var list = await _uow.Repository<F03EmailProfile>().Query().AsNoTracking()
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Code)
            .Select(x => new EmailProfileDto
            {
                Id = x.Id, Code = x.Code, Name = x.Name, EmailServerName = x.EmailServerName,
                EmailServerType = x.EmailServerType, EmailServerPort = x.EmailServerPort,
                EmailServerEnableSsl = x.EmailServerEnableSsl, SecurityMode = x.SecurityMode,
                AuthenticationType = x.AuthenticationType, EmailAccountName = x.EmailAccountName,
                EmailAddress = x.EmailAddress, FromName = x.FromName, ReplyTo = x.ReplyTo,
                SiteUrl = x.SiteUrl, IsDefault = x.IsDefault, IsActive = x.IsActive == true,
                TimeoutSeconds = x.TimeoutSeconds, HasPassword = !string.IsNullOrWhiteSpace(x.EmailPassword)
            }).ToListAsync(ct);
        return Ok(ApiResponse<List<EmailProfileDto>>.Ok(list));
    }

    [HttpPost("profiles")]
    public async Task<IActionResult> SaveProfile([FromBody] EmailProfileDto dto, CancellationToken ct)
    {
        if (!await CanManage(ct)) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.EmailServerName) || string.IsNullOrWhiteSpace(dto.EmailAddress))
            return BadRequest(ApiResponse<object>.Fail("Code, SMTP Server và Email From là bắt buộc."));
        if (dto.EmailServerPort is <= 0 or > 65535) return BadRequest(ApiResponse<object>.Fail("SMTP Port không hợp lệ."));

        var repo = _uow.Repository<F03EmailProfile>();
        var entity = dto.Id == 0
            ? new F03EmailProfile { CreatedBy = UserInfo?.UserId ?? 0, CreatedAt = DateTime.Now }
            : await repo.GetByIdAsync(dto.Id, ct);
        if (entity == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy Email Profile."));
        if (dto.Id == 0) await repo.AddAsync(entity, ct);

        var duplicate = await repo.Query().AnyAsync(x => x.Id != entity.Id && x.Code == dto.Code.Trim().ToUpperInvariant(), ct);
        if (duplicate) return Conflict(ApiResponse<object>.Fail($"Email Profile '{dto.Code}' đã tồn tại."));

        entity.Code = dto.Code.Trim().ToUpperInvariant();
        entity.Name = dto.Name.Trim();
        entity.EmailServerName = dto.EmailServerName.Trim();
        entity.EmailServerType = dto.EmailServerType.Trim().ToUpperInvariant();
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
        entity.IsDefault = dto.IsDefault;
        entity.TimeoutSeconds = Math.Clamp(dto.TimeoutSeconds, 5, 300);
        entity.ModifiedBy = UserInfo?.UserId;
        entity.ModifiedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(dto.Password)) entity.EmailPassword = EncryptUtils.MD5Encrypt(dto.Password);

        if (entity.IsDefault)
        {
            var others = await repo.Query().Where(x => x.Id != entity.Id && x.IsDefault).ToListAsync(ct);
            foreach (var other in others) other.IsDefault = false;
        }

        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok("Đã lưu Email Profile."));
    }

    [HttpPost("profiles/{id:int}/test")]
    public async Task<IActionResult> TestProfile(int id, [FromBody] EmailTestRequest request, CancellationToken ct)
    {
        if (!await CanManage(ct)) return Forbid();
        var profile = await _uow.Repository<F03EmailProfile>().GetByIdAsync(id, ct);
        if (profile == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy Email Profile."));
        if (string.IsNullOrWhiteSpace(request.ToEmail)) return BadRequest(ApiResponse<object>.Fail("Email nhận thử không được để trống."));

        try
        {
            using var smtp = new SmtpClient
            {
                Host = profile.EmailServerName,
                Port = profile.EmailServerPort,
                EnableSsl = profile.EmailServerEnableSsl,
                UseDefaultCredentials = string.Equals(profile.AuthenticationType, "None", StringComparison.OrdinalIgnoreCase),
                Timeout = Math.Clamp(profile.TimeoutSeconds, 5, 300) * 1000
            };
            if (!smtp.UseDefaultCredentials)
                smtp.Credentials = new NetworkCredential(profile.EmailAccountName, string.IsNullOrWhiteSpace(profile.EmailPassword) ? string.Empty : EncryptUtils.MD5Decrypt(profile.EmailPassword));

            using var mail = new MailMessage
            {
                From = new MailAddress(profile.EmailAddress, profile.FromName ?? "FCC Smart Portal"),
                Subject = "[FCC Smart Portal] SMTP Test",
                Body = $"<p>SMTP Profile <strong>{WebUtility.HtmlEncode(profile.Code)}</strong> đã gửi thử thành công.</p><p>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>",
                IsBodyHtml = true
            };
            if (!string.IsNullOrWhiteSpace(profile.ReplyTo)) mail.ReplyToList.Add(profile.ReplyTo);
            mail.To.Add(request.ToEmail.Trim());
            await smtp.SendMailAsync(mail, ct);
            return Ok(ApiResponse.Ok("Kết nối SMTP và gửi email kiểm tra thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] SMTP test failed for profile {Profile}", profile.Code);
            return BadRequest(ApiResponse<object>.Fail($"SMTP test thất bại: {ex.Message}"));
        }
    }

    [HttpGet("policies")]
    public async Task<IActionResult> Policies(CancellationToken ct)
    {
        if (!await CanManage(ct)) return Forbid();
        var list = await _uow.Repository<F03EmailDispatchPolicy>().Query().AsNoTracking()
            .OrderBy(x => x.TemplateCode).ThenBy(x => x.Priority)
            .Select(x => new EmailDispatchPolicyDto
            {
                Id = x.Id, TemplateCode = x.TemplateCode, EmailProfileCode = x.EmailProfileCode,
                DispatchMode = x.DispatchMode.ToString(), Priority = x.Priority,
                IsActive = x.IsActive == true, Description = x.Description
            }).ToListAsync(ct);
        return Ok(ApiResponse<List<EmailDispatchPolicyDto>>.Ok(list));
    }

    [HttpPost("policies")]
    public async Task<IActionResult> SavePolicy([FromBody] EmailDispatchPolicyDto dto, CancellationToken ct)
    {
        if (!await CanManage(ct)) return Forbid();
        if (!Enum.TryParse<EmailDispatchMode>(dto.DispatchMode, true, out var mode)) return BadRequest(ApiResponse<object>.Fail("DispatchMode phải là Disabled, AutoSend hoặc QueueForApproval."));
        var templateExists = await _uow.Repository<F03EmailTemplate>().Query().AnyAsync(x => x.Code == dto.TemplateCode.Trim().ToUpperInvariant() && x.IsActive == true, ct);
        if (!templateExists) return BadRequest(ApiResponse<object>.Fail($"Template '{dto.TemplateCode}' chưa active."));
        var profileExists = await _uow.Repository<F03EmailProfile>().Query().AnyAsync(x => x.Code == dto.EmailProfileCode.Trim().ToUpperInvariant() && x.IsActive == true, ct);
        if (!profileExists) return BadRequest(ApiResponse<object>.Fail($"Email Profile '{dto.EmailProfileCode}' chưa active."));

        var repo = _uow.Repository<F03EmailDispatchPolicy>();
        var entity = dto.Id == 0
            ? new F03EmailDispatchPolicy { CreatedBy = UserInfo?.UserId ?? 0, CreatedAt = DateTime.Now }
            : await repo.GetByIdAsync(dto.Id, ct);
        if (entity == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy chính sách email."));
        if (dto.Id == 0) await repo.AddAsync(entity, ct);

        entity.TemplateCode = dto.TemplateCode.Trim().ToUpperInvariant();
        entity.EmailProfileCode = dto.EmailProfileCode.Trim().ToUpperInvariant();
        entity.DispatchMode = mode;
        entity.Priority = dto.Priority;
        entity.IsActive = dto.IsActive;
        entity.Description = dto.Description?.Trim();
        entity.ModifiedBy = UserInfo?.UserId;
        entity.ModifiedAt = DateTime.Now;
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok("Đã lưu chính sách gửi email."));
    }

    [HttpPost("policies/{id:int}/toggle")]
    public async Task<IActionResult> TogglePolicy(int id, CancellationToken ct)
    {
        if (!await CanManage(ct)) return Forbid();
        var entity = await _uow.Repository<F03EmailDispatchPolicy>().GetByIdAsync(id, ct);
        if (entity == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy chính sách email."));
        entity.IsActive = !entity.IsActive;
        entity.ModifiedBy = UserInfo?.UserId;
        entity.ModifiedAt = DateTime.Now;
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(entity.IsActive == true ? "Đã kích hoạt." : "Đã vô hiệu hóa."));
    }

    private async Task<bool> CanManage(CancellationToken ct) =>
        UserInfo != null && await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.EmailTemplateManage, ct);
}

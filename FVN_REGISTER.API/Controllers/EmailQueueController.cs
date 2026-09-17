using AutoMapper;
using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Contract.Interfaces.Emails;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Models.Data;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmailQueueController : BaseApiController
    {
        private readonly IEmailService _emailService;
        private readonly FVNWEBAPPContext _db;

        public EmailQueueController(
            IEmailService emailService,
            FVNWEBAPPContext db,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<EmailQueueController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _emailService = emailService;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            if (!UserInfo!.IsAdmin()) return Forbid();
            var list = await _db.EmailQueues
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Take(500)
                .Select(x => new EmailQueueDto
                {
                    Id = x.Id,
                    ToEmail = x.ToEmail,
                    TemplateCode = x.TemplateCode,
                    Payload = x.Payload,
                    Status = x.Status,
                    RetryCount = x.RetryCount,
                    MaxRetry = x.MaxRetry,
                    ErrorMessage = x.ErrorMessage,
                    CreatedAt = x.CreatedAt,
                    SentAt = x.SentAt
                })
                .ToListAsync(ct);
            return Ok(ApiResponse<List<EmailQueueDto>>.Ok(list));
        }

        [HttpPost("{id:int}/retry")]
        public async Task<IActionResult> Retry(int id, CancellationToken ct)
        {
            if (!UserInfo!.IsAdmin()) return Forbid();
            await _emailService.ResendEmail(id, ct);
            return Ok(ApiResponse.Ok("Đã đặt lại để gửi."));
        }

        [HttpPost("retry-batch")]
        public async Task<IActionResult> RetryBatch(
            [FromBody] BatchIdsRequest req, CancellationToken ct)
        {
            if (!UserInfo!.IsAdmin()) return Forbid();
            var items = await _db.EmailQueues
                .Where(x => req.Ids.Contains(x.Id))
                .ToListAsync(ct);
            foreach (var e in items)
            {
                e.Status = "Pending";
                e.RetryCount = 0;
                e.ErrorMessage = null;
            }
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse.Ok($"Đã reset {items.Count} email."));
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            if (!UserInfo!.IsAdmin()) return Forbid();
            await _emailService.CancelEmail(id, ct);
            return Ok(ApiResponse.Ok("Đã hủy email."));
        }

        [HttpPost("cancel-batch")]
        public async Task<IActionResult> CancelBatch(
            [FromBody] BatchIdsRequest req, CancellationToken ct)
        {
            if (!UserInfo!.IsAdmin()) return Forbid();
            var items = await _db.EmailQueues
                .Where(x => req.Ids.Contains(x.Id) &&
                            x.Status != "Sent")
                .ToListAsync(ct);
            foreach (var e in items) e.Status = "Cancelled";
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse.Ok($"Đã hủy {items.Count} email."));
        }

        [HttpPost("trigger")]
        public async Task<IActionResult> Trigger(CancellationToken ct)
        {
            if (!UserInfo!.IsAdmin()) return Forbid();
            await _emailService.ProcessQueue(ct);
            return Ok(ApiResponse.Ok("Queue đã được xử lý."));
        }
    }

    public record BatchIdsRequest(List<int> Ids);
}

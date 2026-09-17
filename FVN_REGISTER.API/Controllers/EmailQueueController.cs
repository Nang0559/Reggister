using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmailQueueController : BaseApiController
    {
        private readonly IEmailService _emailService;

        public EmailQueueController(
            IEmailService emailService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<EmailQueueController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsAdmin()) return Forbid();

            var list = await _emailService.GetQueueAsync(500, ct);
            return Ok(ApiResponse<List<EmailQueueDto>>.Ok(list));
        }

        [HttpPost("{id:int}/retry")]
        public async Task<IActionResult> Retry(int id, CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsAdmin()) return Forbid();
            await _emailService.ResendEmail(id, ct);
            return Ok(ApiResponse.Ok("Đã đặt lại để gửi."));
        }

        [HttpPost("retry-batch")]
        public async Task<IActionResult> RetryBatch(
            [FromBody] BatchIdsRequest req, CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsAdmin()) return Forbid();
            await _emailService.RetryEmailBatchAsync(req.Ids, ct);
            return Ok(ApiResponse.Ok($"Đã reset {req.Ids.Count} email."));
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsAdmin()) return Forbid();
            await _emailService.CancelEmail(id, ct);
            return Ok(ApiResponse.Ok("Đã hủy email."));
        }

        [HttpPost("cancel-batch")]
        public async Task<IActionResult> CancelBatch(
            [FromBody] BatchIdsRequest req, CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsAdmin()) return Forbid();
            await _emailService.CancelEmailBatchAsync(req.Ids, ct);
            return Ok(ApiResponse.Ok($"Đã hủy {req.Ids.Count} email."));
        }

        [HttpPost("trigger")]
        public async Task<IActionResult> Trigger(CancellationToken ct)
        {
            if (UserInfo == null || !UserInfo.IsAdmin()) return Forbid();
            await _emailService.ProcessQueue(ct);
            return Ok(ApiResponse.Ok("Queue đã được xử lý."));
        }
    }

    public record BatchIdsRequest(List<int> Ids);
}

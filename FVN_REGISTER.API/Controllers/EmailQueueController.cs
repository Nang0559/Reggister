using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Contract.Requests.Email;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmailQueueController : BaseApiController
{
    private readonly IEmailService _emailService;
    private readonly IAuthorizationService _authorization;

    public EmailQueueController(IEmailService emailService, ICurrentUserService currentUser, IUserLogService userLog, IAuthorizationService authorization, ILogger<EmailQueueController> logger, IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options) { _emailService = emailService; _authorization = authorization; }

    private async Task<bool> CanManage(CancellationToken ct)
        => UserInfo != null && await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.EmailQueueManage, ct);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    { if (!await CanManage(ct)) return Forbid(); return Ok(ApiResponse<List<EmailQueueDto>>.Ok(await _emailService.GetQueueAsync(500, ct))); }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    { if (!await CanManage(ct)) return Forbid(); await _emailService.ApproveEmail(id, UserInfo!.UserId, ct); return Ok(ApiResponse.Ok("Đã duyệt email. Email sẽ được gửi bởi Email Worker.")); }

    [HttpPost("{id:int}/retry")]
    public async Task<IActionResult> Retry(int id, CancellationToken ct)
    { if (!await CanManage(ct)) return Forbid(); await _emailService.ResendEmail(id, ct); return Ok(ApiResponse.Ok("Đã đặt lại để gửi.")); }

    [HttpPost("retry-batch")]
    public async Task<IActionResult> RetryBatch([FromBody] BatchIdsRequestDto req, CancellationToken ct)
    { if (!await CanManage(ct)) return Forbid(); if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Danh sách ID không hợp lệ.")); await _emailService.RetryEmailBatchAsync(req.Ids, ct); return Ok(ApiResponse.Ok($"Đã reset {req.Ids.Count} email.")); }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    { if (!await CanManage(ct)) return Forbid(); await _emailService.CancelEmail(id, ct); return Ok(ApiResponse.Ok("Đã hủy email.")); }

    [HttpPost("cancel-batch")]
    public async Task<IActionResult> CancelBatch([FromBody] BatchIdsRequestDto req, CancellationToken ct)
    { if (!await CanManage(ct)) return Forbid(); if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Danh sách ID không hợp lệ.")); await _emailService.CancelEmailBatchAsync(req.Ids, ct); return Ok(ApiResponse.Ok($"Đã hủy {req.Ids.Count} email.")); }

    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger(CancellationToken ct)
    { if (!await CanManage(ct)) return Forbid(); await _emailService.ProcessQueue(ct); return Ok(ApiResponse.Ok("Queue đã được xử lý.")); }
}

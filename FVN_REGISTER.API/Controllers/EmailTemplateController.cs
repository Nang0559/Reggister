using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.EmailTemplates;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize(Policy = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTemplateController : BaseApiController
    {
        private readonly IEmailTemplateManagementService _templateService;

        public EmailTemplateController(
            IEmailTemplateManagementService templateService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<EmailTemplateController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _templateService = templateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            return Ok(
                ApiResponse<List<EmailTemplateDto>>.Ok(
                    await _templateService.GetAllAsync(ct)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(
            int id,
            CancellationToken ct)
        {
            var item = await _templateService.GetByIdAsync(id, ct);

            return item == null
                ? NotFound()
                : Ok(ApiResponse<EmailTemplateDto>.Ok(item));
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save(
            [FromBody] EmailTemplateDto dto,
            CancellationToken ct)
        {
            var user = UserInfo;
            if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));

            return HandleResult(
                await _templateService.SaveAsync(
                    dto,
                    user.UserId,
                    ct));
        }

        [HttpPost("{id:int}/toggle")]
        public async Task<IActionResult> Toggle(
            int id,
            CancellationToken ct)
        {
            var user = UserInfo;
            if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));

            return HandleResult(
                await _templateService.ToggleActiveAsync(
                    id,
                    user.UserId,
                    ct));
        }
    }
}

using AutoMapper;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ICurrentUserService _currentUser;
        protected readonly IUserLogService _userLog;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;
        protected readonly IOptionsMonitor<AuthDebugOptions> _options;

        protected bool Debug => _options.CurrentValue.Enabled;

        protected UserIdentityDto? UserInfo => _currentUser.GetCurrentUser();

        protected string Path => HttpContext?.Request?.Path.Value ?? "unknown";

        protected BaseApiController(
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _currentUser = currentUser;
            _userLog = userLog;
            _mapper = mapper;
            _logger = logger;
            _options = options;
        }

        // =============================
        // HANDLE RESULT (GENERIC)
        // =============================
        protected ActionResult HandleResult<T>(ServiceResult<T> result)
        {
            if (result == null)
            {
                _logger.LogWarnIf(Debug,
                    "[API] Null result at {Path}",
                    Path);

                return NotFound(ApiResponse<T>.Fail("Không tìm thấy phản hồi từ hệ thống."));
            }

            var apiResponse = ApiResponse<T>.FromResult(result);

            if (result.IsSuccess)
            {
                _logger.LogDebugIf(Debug,
                    "[API] Success: {Path}",
                    Path);

                return Ok(apiResponse);
            }

            _logger.LogWarnIf(Debug,
                "[API] Failed: {Path} | Message: {Message}",
                Path,
                result.Message);

            return BadRequest(apiResponse);
        }

        // =============================
        // HANDLE RESULT (NO DATA)
        // =============================
        protected ActionResult HandleResult(ServiceResult result)
        {
            var apiResponse = ApiResponse<object>.FromResult(result);

            if (result.IsSuccess)
            {
                _logger.LogDebugIf(Debug,
                    "[API] Success: {Path}",
                    Path);

                return Ok(apiResponse);
            }

            _logger.LogWarnIf(Debug,
                "[API] Failed: {Path} | Message: {Message}",
                Path,
                result.Message);

            return BadRequest(apiResponse);
        }

        // =============================
        // PAGED RESULT
        // =============================
        protected ActionResult HandlePagedResult<T>(ServiceResult<PaginationResult<T>> result)
        {
            if (result == null || result.Data == null)
            {
                _logger.LogWarnIf(Debug,
                    "[API] Empty pagination result at {Path}",
                    Path);

                return Ok(ApiResponse<PaginationResult<T>>.Ok(new PaginationResult<T>()));
            }

            return HandleResult(result);
        }

        // =============================
        // USER ACTION LOG
        // =============================
        protected async Task LogActionAsync(string description)
        {
            var user = UserInfo;

            _logger.LogDebugIf(Debug,
                "[USER ACTION] UserId={UserId} | {Desc} | Path={Path}",
                user?.UserId,
                description,
                Path);

            if (user != null)
            {
                await _userLog.UpdateLastSeenAsync(
                    user.UserId,
                    description,
                    Path
                );
            }
        }

        // =============================
        // DEBUG AUTH TEST
        // =============================
        [HttpGet("test-auth")]
        [AllowAnonymous]
        public IActionResult TestAuth()
        {
            var user = HttpContext.User;

            _logger.LogDebugIf(Debug,
                "[AUTH TEST] IsAuth={Auth} | Name={Name}",
                user.Identity?.IsAuthenticated,
                user.Identity?.Name);

            return Ok(new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated,
                Name = user.Identity?.Name,
                AuthenticationType = user.Identity?.AuthenticationType,
                Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                UserInfoId = UserInfo?.UserId
            });
        }
    }
}
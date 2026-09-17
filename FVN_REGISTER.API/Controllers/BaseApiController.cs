using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ICurrentUserService _currentUser;
        protected readonly IUserLogService _userLog;
        protected readonly ILogger _logger;
        protected readonly IOptionsMonitor<AuthDebugOptions> _options;

        protected bool Debug => _options.CurrentValue.Enabled;
        protected UserIdentityDto? UserInfo => _currentUser.GetCurrentUser();
        protected string Path => HttpContext?.Request?.Path.Value ?? "unknown";

        protected BaseApiController(
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _currentUser = currentUser;
            _userLog = userLog;
            _logger = logger;
            _options = options;
        }

        protected ActionResult HandleResult<T>(ServiceResult<T> result)
        {
            if (result == null)
            {
                _logger.LogWarnIf(Debug, "[API] Null result at {Path}", Path);
                return NotFound(ApiResponse<T>.Fail("Không tìm thấy phản hồi từ hệ thống."));
            }

            var apiResponse = ApiResponse<T>.FromResult(result);
            if (result.IsSuccess)
            {
                _logger.LogDebugIf(Debug, "[API] Success: {Path}", Path);
                return Ok(apiResponse);
            }

            _logger.LogWarnIf(Debug,
                "[API] Failed: {Path} | Message: {Message}",
                Path,
                result.Message);
            return BadRequest(apiResponse);
        }

        protected ActionResult HandleResult(ServiceResult result)
        {
            var apiResponse = ApiResponse<object>.FromResult(result);
            if (result.IsSuccess)
            {
                _logger.LogDebugIf(Debug, "[API] Success: {Path}", Path);
                return Ok(apiResponse);
            }

            _logger.LogWarnIf(Debug,
                "[API] Failed: {Path} | Message: {Message}",
                Path,
                result.Message);
            return BadRequest(apiResponse);
        }

        protected ActionResult HandlePagedResult<T>(ServiceResult<PaginationResult<T>> result)
        {
            if (result == null || result.Data == null)
            {
                _logger.LogWarnIf(Debug, "[API] Empty pagination result at {Path}", Path);
                return Ok(ApiResponse<PaginationResult<T>>.Ok(new PaginationResult<T>()));
            }

            return HandleResult(result);
        }

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
                await _userLog.UpdateLastSeenAsync(user.UserId, description, Path);
            }
        }
    }
}

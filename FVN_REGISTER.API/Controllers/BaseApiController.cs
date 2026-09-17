using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Application.Logging;

namespace FVN_REGISTER.API.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ICurrentUserService _currentUser;
        protected readonly IUserLogService _userLog;
        protected readonly ILogger _logger;
        protected readonly IOptionsMonitor<FVN_REGISTER.Application.Configuration.AuthDebugOptions> _options;

        protected bool Debug => _options.CurrentValue.Enabled;
        protected UserIdentityDto? UserInfo => _currentUser.GetCurrentUser();
        protected string Path => HttpContext?.Request?.Path.Value ?? "unknown";

        protected BaseApiController(
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger logger,
            IOptionsMonitor<FVN_REGISTER.Application.Configuration.AuthDebugOptions> options)
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
                if (Debug) _logger.LogWarning("[API] Null result at {Path}", Path);
                return NotFound(ApiResponse<T>.Fail("Không tìm thấy phản hồi từ hệ thống."));
            }

            var apiResponse = ApiResponse<T>.FromResult(result);
            if (result.IsSuccess)
            {
                if (Debug) _logger.LogDebug("[API] Success: {Path}", Path);
                return Ok(apiResponse);
            }

            if (Debug) _logger.LogWarning("[API] Failed: {Path} | Message: {Message}", Path, result.Message);
            return BadRequest(apiResponse);
        }

        protected ActionResult HandleResult(ServiceResult result)
        {
            var apiResponse = ApiResponse<object>.FromResult(result);
            if (result.IsSuccess)
            {
                if (Debug) _logger.LogDebug("[API] Success: {Path}", Path);
                return Ok(apiResponse);
            }

            if (Debug) _logger.LogWarning("[API] Failed: {Path} | Message: {Message}", Path, result.Message);
            return BadRequest(apiResponse);
        }

        protected ActionResult HandlePagedResult<T>(ServiceResult<PaginationResult<T>> result)
        {
            if (result == null || result.Data == null)
            {
                if (Debug) _logger.LogWarning("[API] Empty pagination result at {Path}", Path);
                return Ok(ApiResponse<PaginationResult<T>>.Ok(new PaginationResult<T>()));
            }

            return HandleResult(result);
        }

        protected async Task LogActionAsync(string description)
        {
            var user = UserInfo;
            if (Debug)
            {
                _logger.LogDebug(
                    "[USER ACTION] UserId={UserId} | {Desc} | Path={Path}",
                    user?.UserId,
                    description,
                    Path);
            }

            if (user != null)
            {
                await _userLog.UpdateLastSeenAsync(user.UserId, description, Path);
            }
        }
    }
}

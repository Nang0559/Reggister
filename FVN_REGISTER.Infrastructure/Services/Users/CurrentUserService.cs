using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;


namespace FVN_REGISTER.Infrastructure.Services.Users
{
    public class CurrentUserService
       : BaseService<CurrentUserService>, ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CurrentUserService> logger,
            IOptionsMonitor<AuthDebugOptions> options
        ) : base(logger, options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsLoggedIn =>
            _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

        public UserIdentityDto? GetCurrentUser()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !user.Identity!.IsAuthenticated)
                {
                    Logger.LogDebugIf(Debug, "[CURRENT_USER] Not authenticated");
                    return null;
                }

                var userIdStr = user.FindFirst("UserId")?.Value
                             ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdStr, out var userId))
                {
                    Logger.LogWarnIf(Debug, "[CURRENT_USER] Invalid UserId claim: {Value}", userIdStr);
                    return null;
                }

                int.TryParse(user.FindFirst("LevelApprove")?.Value, out var levelApprove);

                // Ưu tiên claim PermissionCode — đây là nguồn CHẮC CHẮN tồn tại
                // (AuthService luôn phát hành), roles chỉ là dẫn xuất phụ từ nó.
                var permission = UserRole.None;

                if (int.TryParse(user.FindFirst("PermissionCode")?.Value, out var permCode))
                {
                    permission = (UserRole)permCode;
                }
                else
                {
                    // Fallback: dựng lại từ role claims nếu vì lý do gì đó PermissionCode vắng mặt
                    foreach (var roleClaim in user.FindAll(ClaimTypes.Role))
                    {
                        if (Enum.TryParse<UserRole>(roleClaim.Value, out var role))
                            permission |= role;
                    }
                }

                var functions = user.FindAll("Function")
                    .Select(c => int.TryParse(c.Value, out var f) ? f : 0)
                    .Where(x => x != 0)
                    .ToList();

                var currentUser = new UserIdentityDto
                {
                    UserId = userId,
                    UserName = user.FindFirst("EmployeeCode")?.Value ?? "",
                    FullName = user.FindFirst("FullName")?.Value ?? "",
                    EmployeeCode = user.FindFirst("EmployeeCode")?.Value ?? "",
                    DeptCode = user.FindFirst("DeptCode")?.Value ?? "",
                    PositionCode = user.FindFirst("PositionCode")?.Value ?? "",
                    Email = user.FindFirst("Email")?.Value ?? "",
                    LevelApprove = levelApprove,
                    Permission = (int)permission,
                    Functions = functions
                };

                Logger.LogDebugIf(Debug,
                    "[CURRENT_USER] Loaded: UserId={UserId} | Permission={Permission}",
                    userId, permission);

                return currentUser;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[CURRENT_USER] Parse error");
                return null;
            }
        }

        public string? GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var id = user?.FindFirst("UserId")?.Value
                  ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Debug && string.IsNullOrEmpty(id))
            {
                Logger.LogWarnIf(Debug, "[CURRENT_USER] GetUserId null");
            }

            return id;
        }
    }
}

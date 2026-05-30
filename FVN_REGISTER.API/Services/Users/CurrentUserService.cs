using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.Extensions.Options;
using System.Security.Claims;


namespace FVN_REGISTER.API.Services.Users
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

        public CurrentUser? GetCurrentUser()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !user.Identity!.IsAuthenticated)
                {
                    Logger.LogDebugIf(Debug, "[CURRENT_USER] Not authenticated");
                    return null;
                }

                var userIdStr = user.FindFirst("nameid")?.Value
                             ?? user.FindFirst("UserId")?.Value;

                var userName = user.FindFirst("unique_name")?.Value
                            ?? user.Identity?.Name;

                if (!int.TryParse(userIdStr, out var userId))
                {
                    Logger.LogWarnIf(Debug, "[CURRENT_USER] Invalid UserId claim: {Value}", userIdStr);
                    return null;
                }

                var levelApprove = 0;
                int.TryParse(user.FindFirst("LevelApprove")?.Value, out levelApprove);

                // ✅ Dùng UserRole enum [Flags] để parse Permission từ roles trong JWT
                var permission = UserRole.None;
                foreach (var roleClaim in user.FindAll(ClaimTypes.Role))
                {
                    if (Enum.TryParse<UserRole>(roleClaim.Value, out var role))
                        permission |= role;
                }

                // Fallback: đọc thẳng từ claim PermissionCode nếu có
                if (permission == UserRole.None &&
                    int.TryParse(user.FindFirst("PermissionCode")?.Value, out var permCode))
                {
                    permission = (UserRole)permCode;
                }

                var functions = user.FindAll("Function")
                    .Select(c => int.TryParse(c.Value, out var f) ? f : 0)
                    .Where(x => x != 0)
                    .ToList();

                var currentUser = new CurrentUser
                {
                    UserId = userId,
                    UserName = userName ?? "",
                    Email = user.FindFirst("Email")?.Value
                         ?? user.FindFirst(ClaimTypes.Email)?.Value
                         ?? "",
                    FullName = user.FindFirst("FullName")?.Value ?? "",
                    EmployeeCode = user.FindFirst("EmployeeCode")?.Value ?? "",
                    DeptCode = user.FindFirst("DeptCode")?.Value ?? "",
                    CvCode = user.FindFirst("CvCode")?.Value ?? "",
                    LevelApprove = levelApprove,
                    Permission = (int)permission,  // ✅ Cast UserRole → int
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

            var id = user?.FindFirst("nameid")?.Value
                  ?? user?.FindFirst("UserId")?.Value;

            if (Debug && string.IsNullOrEmpty(id))
            {
                Logger.LogWarnIf(Debug, "[CURRENT_USER] GetUserId null");
            }

            return id;
        }
    }
}

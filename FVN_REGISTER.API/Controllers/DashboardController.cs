using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : BaseApiController
    {
        private readonly IDashboardOrchestrator _dashboardOrchestrator;
        private readonly FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService _authorization;

        public DashboardController(
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<DashboardController> logger,
            IOptionsMonitor<AuthDebugOptions> options,
            IDashboardOrchestrator dashboardOrchestrator,
            FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService authorization)
            : base(currentUser, userLog, logger, options)
        {
            _dashboardOrchestrator = dashboardOrchestrator;
            _authorization = authorization;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, FVN_REGISTER.Core.Constants.SecurityFunctionCodes.DashboardView, ct))
                return Forbid();

            var result = await _dashboardOrchestrator.BuildAsync(UserInfo, ct);
            await LogActionAsync("Xem dữ liệu Dashboard");
            return HandleResult(result);
        }
    }
}

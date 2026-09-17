using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Configurations;
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
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<DashboardController> logger,
            IOptionsMonitor<AuthDebugOptions> options,
            IDashboardService dashboardService)
            : base(currentUser, userLog, logger, options)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));

            var result = await _dashboardService.GetDashboardAsync(UserInfo, ct);
            await LogActionAsync("Xem dữ liệu Dashboard");
            return HandleResult(result);
        }
    }
}

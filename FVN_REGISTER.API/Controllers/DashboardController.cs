using AutoMapper;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Statics;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            IMapper mapper,
            ILogger<DashboardController> logger,
            IOptionsMonitor<AuthDebugOptions> options,
            IDashboardService dashboardService)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(CancellationToken ct)
        {
            if (UserInfo == null)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));
            }

            var result = await _dashboardService.GetDashboardAsync(UserInfo, ct);

            await LogActionAsync("Xem dữ liệu Dashboard");

            return HandleResult(result);
        }
    }
}

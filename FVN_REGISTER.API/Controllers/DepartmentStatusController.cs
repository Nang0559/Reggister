using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/department-status")]
    public class DepartmentStatusController : BaseApiController
    {
        private readonly IDepartmentStatusService _deptStatus;
        private readonly IAuthorizationService _authorization;

        public DepartmentStatusController(
            IDepartmentStatusService deptStatus,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IAuthorizationService authorization,
            ILogger<DepartmentStatusController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _deptStatus = deptStatus;
            _authorization = authorization;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? date, CancellationToken ct)
        {
            if (UserInfo == null || !await _authorization.CanAccessAsync(UserInfo, SecurityFunctionCodes.DepartmentStatusView, null, UserInfo.DeptCode, ct)) return Forbid();
            var result = await _deptStatus.GetAllDeptStatusAsync(date ?? DateTime.Today, ct);
            return Ok(ApiResponse<object>.Ok(result));
        }
    }
}

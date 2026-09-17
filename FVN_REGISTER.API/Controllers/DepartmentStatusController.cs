using AutoMapper;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/department-status")]
    public class DepartmentStatusController : BaseApiController
    {
        private readonly IDepartmentStatusService _deptStatus;

        public DepartmentStatusController(
            IDepartmentStatusService deptStatus,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<DepartmentStatusController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _deptStatus = deptStatus;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            var result = await _deptStatus.GetAllAsync(date ?? DateTime.Today, ct);
            return HandleResult(result);
        }
    }
}

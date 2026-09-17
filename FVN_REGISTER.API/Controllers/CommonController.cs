using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommonController : BaseApiController
    {
        private readonly IDepartmentLookupService _departmentLookup;

        public CommonController(
            IDepartmentLookupService departmentLookup,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<CommonController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _departmentLookup = departmentLookup;
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments(CancellationToken ct)
        {
            var list = await _departmentLookup.GetActiveDepartmentsAsync(ct);
            return Ok(ApiResponse<List<DeptOption>>.Ok(list));
        }
    }
}

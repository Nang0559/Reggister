using AutoMapper;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Models.Data;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommonController : BaseApiController
    {
        private readonly FVNWEBAPPContext _db;

        public CommonController(
            FVNWEBAPPContext db,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<CommonController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _db = db;
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments(CancellationToken ct)
        {
            var list = await _db.F03departments
                .AsNoTracking()
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.DeptName)
                .Select(x => new DeptOption
                {
                    DeptCode = x.DeptCode,
                    DeptName = x.DeptName
                })
                .ToListAsync(ct);

            return Ok(ApiResponse<List<DeptOption>>.Ok(list));
        }
    }
}

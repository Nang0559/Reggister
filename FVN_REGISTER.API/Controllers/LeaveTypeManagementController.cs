using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.LeaveTypes;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveTypeManagementController : BaseApiController
    {
        private readonly ILeaveTypeManagementService _leaveTypeService;

        public LeaveTypeManagementController(
            ILeaveTypeManagementService leaveTypeService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<LeaveTypeManagementController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _leaveTypeService = leaveTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => HandleResult(await _leaveTypeService.GetAllAsync(ct));

        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered(
            [FromQuery] bool? tinhPhep,
            [FromQuery] bool? isActive,
            CancellationToken ct)
            => HandleResult(await _leaveTypeService.GetFilteredAsync(tinhPhep, isActive, ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LeaveTypeUpsertDto model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (UserInfo == null) return Unauthorized();
            return HandleResult(await _leaveTypeService.CreateAsync(model, UserInfo.UserId, ct));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] LeaveTypeUpsertDto model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (UserInfo == null) return Unauthorized();

            model.Id = id;
            return HandleResult(await _leaveTypeService.UpdateAsync(model, UserInfo.UserId, ct));
        }

        [HttpPatch("{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            return HandleResult(await _leaveTypeService.ToggleAsync(id, UserInfo.UserId, ct));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
            => HandleResult(await _leaveTypeService.DeleteAsync(id, ct));
    }
}

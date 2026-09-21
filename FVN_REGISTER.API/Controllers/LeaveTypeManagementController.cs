using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.LeaveTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveTypeManagementController : BaseApiController
    {
        private readonly ILeaveTypeManagementService _leaveTypeService;
        private readonly IAuthorizationService _authorization;

        public LeaveTypeManagementController(
            ILeaveTypeManagementService leaveTypeService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IAuthorizationService authorization,
            ILogger<LeaveTypeManagementController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _leaveTypeService = leaveTypeService;
            _authorization = authorization;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            { if (!await CanAsync(SecurityFunctionCodes.LeaveTypeView, ct)) return Forbid(); return HandleResult(await _leaveTypeService.GetAllAsync(ct)); }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] bool? tinhPhep, [FromQuery] bool? isActive, CancellationToken ct)
            { if (!await CanAsync(SecurityFunctionCodes.LeaveTypeView, ct)) return Forbid(); return HandleResult(await _leaveTypeService.GetFilteredAsync(tinhPhep, isActive, ct)); }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LeaveTypeUpsertDto model, CancellationToken ct)
        {
            if (!await CanAsync(SecurityFunctionCodes.LeaveTypeManage, ct)) return Forbid();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (UserInfo == null) return Unauthorized();
            return HandleResult(await _leaveTypeService.CreateAsync(model, UserInfo.UserId, ct));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] LeaveTypeUpsertDto model, CancellationToken ct)
        {
            if (!await CanAsync(SecurityFunctionCodes.LeaveTypeManage, ct)) return Forbid();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (UserInfo == null) return Unauthorized();
            model.Id = id;
            return HandleResult(await _leaveTypeService.UpdateAsync(model, UserInfo.UserId, ct));
        }

        [HttpPatch("{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id, CancellationToken ct)
        {
            if (!await CanAsync(SecurityFunctionCodes.LeaveTypeManage, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized();
            return HandleResult(await _leaveTypeService.ToggleAsync(id, UserInfo.UserId, ct));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        { if (!await CanAsync(SecurityFunctionCodes.LeaveTypeManage, ct)) return Forbid(); return HandleResult(await _leaveTypeService.DeleteAsync(id, ct)); }
        private async Task<bool> CanAsync(int code, CancellationToken ct) => UserInfo != null && await _authorization.HasAsync(UserInfo, code, ct);
    }
}

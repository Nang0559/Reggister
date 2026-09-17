using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.ViewModels.Leaves;
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
        {
            var result = await _leaveTypeService.GetAllAsync(ct);
            return HandleResult(result);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered(
            [FromQuery] bool? tinhPhep,
            [FromQuery] bool? isActive,
            CancellationToken ct)
        {
            var result = await _leaveTypeService.GetFilteredAsync(tinhPhep, isActive, ct);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] LeaveTypeUpsertModel model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (UserInfo == null)
                return Unauthorized();

            await LogActionAsync($"Thêm hình thức nghỉ: {model.LeaveTypeCode}");
            var result = await _leaveTypeService.CreateAsync(model, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] LeaveTypeUpsertModel model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (UserInfo == null)
                return Unauthorized();

            model.LeaveTypeId = id;
            await LogActionAsync($"Sửa hình thức nghỉ: {model.LeaveTypeCode}");
            var result = await _leaveTypeService.UpdateAsync(model, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        [HttpPatch("{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized();

            await LogActionAsync($"Toggle hình thức nghỉ ID={id}");
            var result = await _leaveTypeService.ToggleAsync(id, UserInfo.UserId, ct);
            return HandleResult(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await LogActionAsync($"Xóa hình thức nghỉ ID={id}");
            var result = await _leaveTypeService.DeleteAsync(id, ct);
            return HandleResult(result);
        }
    }
}

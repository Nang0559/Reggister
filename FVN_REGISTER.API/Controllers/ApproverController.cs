using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApproverController : BaseApiController
    {
        private readonly IApproverManagementService _approverService;

        public ApproverController(
            IApproverManagementService approverService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<ApproverController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _approverService = approverService;
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetTree(CancellationToken ct)
        {
            var result = await _approverService.GetApproverTreeAsync(ct);
            await LogActionAsync("Xem danh sách Approver");
            return HandleResult(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? deptCode, [FromQuery] int? level, [FromQuery] RequestModule? requestType, CancellationToken ct)
            => HandleResult(await _approverService.GetListAsync(deptCode, level, requestType, ct));

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments(CancellationToken ct)
            => HandleResult(await _approverService.GetDepartmentsAsync(ct));

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees([FromQuery] string? deptCode, CancellationToken ct)
            => HandleResult(await _approverService.GetEmployeesAsync(deptCode, ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ApproverDto model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _approverService.CreateAsync(model, UserInfo.UserId, ct);
            await LogActionAsync($"Thêm Approver: {model.ApproverName} - Dept: {model.ApproveForDeptCode}");
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ApproverDto model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            model.Id = id;
            var result = await _approverService.UpdateAsync(model, UserInfo.UserId, ct);
            await LogActionAsync($"Sửa Approver ID: {id}");
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _approverService.DeleteAsync(id, UserInfo.UserId, ct);
            await LogActionAsync($"Xóa Approver ID: {id}");
            return HandleResult(result);
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _approverService.ToggleActiveAsync(id, UserInfo.UserId, ct);
            await LogActionAsync($"Toggle Approver ID: {id}");
            return HandleResult(result);
        }
    }
}

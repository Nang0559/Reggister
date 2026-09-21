using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
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
        private readonly IAuthorizationService _authorization;

        public ApproverController(
            IApproverManagementService approverService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IAuthorizationService authorization,
            ILogger<ApproverController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _approverService = approverService;
            _authorization = authorization;
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetTree(CancellationToken ct)
        {
            if (!await CanAsync(SecurityFunctionCodes.ApproverView, ct)) return Forbid();
            var result = await _approverService.GetApproverTreeAsync(ct);
            await LogActionAsync("Xem danh sách Approver");
            return HandleResult(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? deptCode, [FromQuery] int? level, [FromQuery] RequestModule? requestType, CancellationToken ct)
            { if (!await CanAsync(SecurityFunctionCodes.ApproverView, ct)) return Forbid(); return HandleResult(await _approverService.GetListAsync(deptCode, level, requestType, ct));

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments(CancellationToken ct)
            { if (!await CanAsync(SecurityFunctionCodes.ApproverView, ct)) return Forbid(); return HandleResult(await _approverService.GetDepartmentsAsync(ct)); }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees([FromQuery] string? deptCode, CancellationToken ct)
            { if (!await CanAsync(SecurityFunctionCodes.ApproverView, ct)) return Forbid(); return HandleResult(await _approverService.GetEmployeesAsync(deptCode, ct)); }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ApproverDto model, CancellationToken ct)
        {
            if (!await CanAsync(SecurityFunctionCodes.ApproverManage, ct)) return Forbid();
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _approverService.CreateAsync(model, UserInfo.UserId, ct);
            await LogActionAsync($"Thêm Approver: {model.ApproverName} - Dept: {model.ApproveForDeptCode}");
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ApproverDto model, CancellationToken ct)
        {
            if (!await CanAsync(SecurityFunctionCodes.ApproverManage, ct)) return Forbid();
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            model.Id = id;
            var result = await _approverService.UpdateAsync(model, UserInfo.UserId, ct);
            await LogActionAsync($"Sửa Approver ID: {id}");
            return HandleResult(result);
        }

        [HttpGet("sync-proposals")]
        public async Task<IActionResult> GetSyncProposals(CancellationToken ct)
            => HandleResult(await _approverService.GetSyncProposalsAsync(ct));

        [HttpPost("sync-proposals/{flagId:int}/accept")]
        public async Task<IActionResult> AcceptSyncProposal(int flagId, CancellationToken ct)
        {
            if (!await CanAsync(SecurityFunctionCodes.ApproverManage, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _approverService.AcceptSyncProposalAsync(flagId, UserInfo.UserId, ct));
        }

        [HttpPost("sync-proposals/{flagId:int}/keep")]
        public async Task<IActionResult> KeepSyncProposal(int flagId, CancellationToken ct)
        {
            if (!await CanAsync(SecurityFunctionCodes.ApproverManage, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _approverService.KeepSyncProposalAsync(flagId, UserInfo.UserId, ct));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (!await CanAsync(SecurityFunctionCodes.ApproverManage, ct)) return Forbid();
            if (UserInfo == null return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _approverService.DeleteAsync(id, UserInfo.UserId, ct);
            await LogActionAsync($"Xóa Approver ID: {id}");
            return HandleResult(result);
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id, CancellationToken ct)
        {
            if (!await CanAsync(SecurityFunctionCodes.ApproverManage, ct)) return Forbid();
            if (UserInfo == null return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            var result = await _approverService.ToggleActiveAsync(id, UserInfo.UserId, ct);
            await LogActionAsync($"Toggle Approver ID: {id}");
            return HandleResult(result);
        }
        private async Task<bool> CanAsync(int code, CancellationToken ct) => UserInfo != null && await _authorization.HasAsync(UserInfo, code, ct);
    }
}

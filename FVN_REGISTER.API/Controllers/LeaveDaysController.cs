using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveDaysController : BaseApiController
    {
        private readonly IAuthorizationService _authorization;
        private readonly ILeaveService _leaveService;
        private readonly ILeaveQueryService _queryService;
        private readonly IApprovalWorkflowOrchestrator<LeaveRequestSubject> _workflow;

        public LeaveDaysController(
            ILeaveService leaveService,
            ILeaveQueryService queryService,
            IApprovalWorkflowOrchestrator<LeaveRequestSubject> workflow,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<LeaveDaysController> logger,
            IOptionsMonitor<AuthDebugOptions> options,
            FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService authorization)
            : base(currentUser, userLog, logger, options)
        {
            _authorization = authorization;
            _leaveService = leaveService;
            _queryService = queryService;
            _workflow = workflow;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] LeaveRequestUpsertDto model, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveCreate, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return HandleResult(await _leaveService.CreateAsync(model, UserInfo, ct));
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] LeaveCancelRequestDto body, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveCancel, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));
            return HandleResult(await _leaveService.CancelAsync(id, body.Reason, UserInfo, ct));
        }

        [HttpPost("cancel-detail/{detailId:int}")]
        public async Task<IActionResult> CancelDetail(int detailId, [FromBody] LeaveDetailCancelRequestDto request, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveCancel, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));
            return HandleResult(await _leaveService.CancelDetailAsync(detailId, request.Reason, UserInfo, ct));
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] LeaveApprovalCommandDto request, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveApprove, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            if (request.Ids == null || request.Ids.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Chưa chọn đơn nào."));
            return HandleResult(await _leaveService.ApproveAsync(request.Ids, request.Level, UserInfo, request.Comment, ct));
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] LeaveApprovalCommandDto request, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveApprove, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            if (request.Ids == null || request.Ids.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Chưa chọn đơn nào."));
            if (string.IsNullOrWhiteSpace(request.Comment)) return BadRequest(ApiResponse<object>.Fail("Lý do từ chối không được để trống."));
            return HandleResult(await _leaveService.RejectAsync(request.Ids, request.Level, UserInfo, request.Comment, ct));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveApprove, ct)) return Forbid();
            if (UserInfo?.Email == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return Ok(ApiResponse<List<PendingApprovalItemDto>>.Ok(await _workflow.GetPendingForApproverAsync(UserInfo.Email, ct)));
        }
        [HttpGet("pending-summary")]
        public async Task<IActionResult> GetPendingSummary(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveApprove, ct)) return Forbid();
            if (UserInfo?.Email == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var items = await _workflow.GetPendingForApproverAsync(UserInfo.Email, ct);

            var summary = items
                .GroupBy(x => x.ApprovalSteps
                    .Where(s => s.IsRequired && s.Decision == DecisionType.Pending)
                    .OrderBy(s => s.Level)
                    .Select(s => s.Level)
                    .FirstOrDefault())
                .OrderBy(x => x.Key)
                .Select(x => new
                {
                    Level = x.Key,
                    Count = x.Count()
                })
                .ToList();

            return Ok(ApiResponse<object>.Ok(summary));
        }

        [HttpGet("balance/{year:int}")]
        public async Task<IActionResult> GetBalance(int year, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveView, ct)) return Forbid();
            if (UserInfo?.EmployeeCode == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            return Ok(ApiResponse<LeaveBalanceDto>.Ok(
                await _queryService.GetSimpleBalanceAsync(UserInfo.EmployeeCode, year, ct)));
        }

        [HttpGet("details/{id:int}")]
        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> GetDetails(int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveView, ct)) return Forbid();
            return HandleResult(await _queryService.GetFullDetailsAsync(id, ct));
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int? year, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveView, ct)) return Forbid();
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveView, ct)) return Forbid();
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            if (string.IsNullOrWhiteSpace(UserInfo.DeptCode))
                return BadRequest(ApiResponse<object>.Fail("Tài khoản chưa được gán phòng ban."));

            ApprovalStatus? parsedStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApprovalStatus>(status, true, out var statusValue)) parsedStatus = statusValue;
            var selectedYear = year ?? DateTime.Now.Year;
            var result = await _queryService.GetPagedAsync(UserInfo.DeptCode, parsedStatus, new DateTime(selectedYear, 1, 1), new DateTime(selectedYear, 12, 31), page, pageSize, ct);
            return HandleResult(ServiceResult<PaginationResult<LeaveSummaryDto>>.Ok(result));
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int limit = 5, CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.LeaveView, ct)) return Forbid();
            if (UserInfo?.EmployeeCode == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return Ok(ApiResponse<List<LeaveSummaryDto>>.Ok(await _queryService.GetRecentSummaryAsync(UserInfo.EmployeeCode, limit, ct)));
        }
    }
}

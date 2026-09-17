using AutoMapper;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Models.ApprovalModels;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels.Approvals;
using FVN_REGISTER.Core.Configurations;
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
        private readonly ILeaveService _leaveService;

        public LeaveDaysController(
            ILeaveService leaveService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<LeaveDaysController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _leaveService = leaveService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateLeaveRequestModel model, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            return HandleResult(await _leaveService.CreateLeaveAsync(model, UserInfo, ct));
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelLeaveBody body, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            return HandleResult(await _leaveService.CancelAsync(id, body.Reason, UserInfo, ct));
        }

        [HttpPost("cancel-detail/{detailId:int}")]
        public async Task<IActionResult> CancelDetail(int detailId, [FromBody] CancelDetailRequest request, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            return HandleResult(await _leaveService.CancelDetailAsync(detailId, request.Reason, UserInfo, ct));
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] ApproveRequest req, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            return HandleResult(await _leaveService.ApproveAsync(req.Ids, req.Level, UserInfo, req.Comment ?? "", ct));
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] ApproveRequest req, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            if (string.IsNullOrWhiteSpace(req.Comment)) return BadRequest(ApiResponse<object>.Fail("Lý do từ chối không được để trống."));
            return HandleResult(await _leaveService.RejectAsync(req.Ids, req.Level, UserInfo, req.Comment, ct));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken ct)
        {
            if (UserInfo?.Email == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _leaveService.GetMyPendingAsync(UserInfo.Email, ct));
        }

        [HttpGet("pending-summary")]
        public async Task<IActionResult> GetPendingSummary(CancellationToken ct)
        {
            if (UserInfo?.Email == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));
            return HandleResult(await _leaveService.GetMyPendingSummaryAsync(UserInfo.Email, ct));
        }

        [HttpGet("balance/{year:int}")]
        public async Task<IActionResult> GetBalance(int year, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            return HandleResult(await _leaveService.GetBalanceAsync(year, UserInfo, ct));
        }

        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> GetDetails(int id, CancellationToken ct)
            => HandleResult(await _leaveService.GetDetailsAsync(id, ct));

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int? year, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            var filter = new HistoryFilterDto { Year = year, Status = status, Page = page, PageSize = pageSize };
            return HandleResult(await _leaveService.GetHistoryAsync(filter, UserInfo, ct));
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int limit = 5, CancellationToken ct = default)
        {
            if (UserInfo?.EmployeeCode == null) return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));
            return HandleResult(await _leaveService.GetRecentRequestsAsync(UserInfo.EmployeeCode, limit, ct));
        }
    }

    public record CancelLeaveBody(string Reason);

    public class CancelDetailRequest
    {
        public string Reason { get; set; } = "";
    }
}

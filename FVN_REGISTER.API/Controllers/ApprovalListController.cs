using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalListController : BaseApiController
    {
        private readonly IApprovalInboxService _approvalInbox;

        public ApprovalListController(
            IApprovalInboxService approvalInbox,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<ApprovalListController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _approvalInbox = approvalInbox;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            return HandleResult(await _approvalInbox.GetPendingAsync(UserInfo, ct));
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] ApprovalActionDto req, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            if (req.RequestIds == null || req.RequestIds.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Chưa chọn đơn nào."));
            if (req.IsReject)
                return BadRequest(ApiResponse<object>.Fail("Endpoint approve không nhận IsReject=true."));

            var result = await _approvalInbox.ApproveItemsAsync(
                req.RequestIds, req.Kind, req.Level, req.Comment, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] ApprovalActionDto req, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            if (req.RequestIds == null || req.RequestIds.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Chưa chọn đơn nào."));
            if (string.IsNullOrWhiteSpace(req.Comment))
                return BadRequest(ApiResponse<object>.Fail("Vui lòng nhập lý do từ chối."));
            if (!req.IsReject)
                return BadRequest(ApiResponse<object>.Fail("Endpoint reject yêu cầu IsReject=true."));

            var result = await _approvalInbox.RejectItemsAsync(
                req.RequestIds, req.Kind, req.Level, req.Comment, UserInfo, ct);
            return HandleResult(result);
        }
    }
}

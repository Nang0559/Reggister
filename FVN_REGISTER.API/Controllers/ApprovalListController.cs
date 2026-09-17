using AutoMapper;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces.Approvals;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    // FVN_REGISTER.API/Controllers/ApprovalListController.cs
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalListController : BaseApiController
    {
        private readonly IApprovalInboxService _approvalList;

        public ApprovalListController(
            IApprovalInboxService approvalList,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<ApprovalListController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _approvalList = approvalList;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var result = await _approvalList.GetPendingAsync(UserInfo, ct);
            return Ok(ApiResponse<PendingApprovalListDto>.Ok(result));
        }
        [HttpGet("{kind}/{id:int}")]
        public async Task<IActionResult> GetById(string kind, int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var result = await _approvalList.GetByIdAsync(kind, id, UserInfo, ct);
            return HandleResult(result);
        }
        [HttpPost("approve")]
        public async Task<IActionResult> Approve(
     [FromBody] ApprovalActionRequest req,
     CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            if (req.Ids == null || req.Ids.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Chưa chọn đơn nào."));

            var result = await _approvalList.ApproveItemsAsync(
                req.Ids, req.Kind, req.Level, req.Comment, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject(
            [FromBody] ApprovalActionRequest req,
            CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            if (req.Ids == null || req.Ids.Count == 0)
                return BadRequest(ApiResponse<object>.Fail("Chưa chọn đơn nào."));
            if (string.IsNullOrWhiteSpace(req.Comment))
                return BadRequest(ApiResponse<object>.Fail("Vui lòng nhập lý do từ chối."));

            var result = await _approvalList.RejectItemsAsync(
                req.Ids, req.Kind, req.Level, req.Comment!, UserInfo, ct);
            return HandleResult(result);
        }
    }

   
}

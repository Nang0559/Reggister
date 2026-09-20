using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Requests;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : BaseApiController
    {
        private readonly IHistoryDispatcher _dispatcher;

        public HistoryController(
            IHistoryDispatcher dispatcher,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            ILogger<HistoryController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, logger, options)
        {
            _dispatcher = dispatcher;
        }

        [HttpGet("{kind}")]
        public async Task<IActionResult> GetHistory(
            string kind,
            [FromQuery] int? year,
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (UserInfo == null) return Unauthorized();
            if (!TryParseRequestKind(kind, out var requestKind))
                return BadRequest(ApiResponse<object>.Fail("Loại đơn không hợp lệ. Chỉ hỗ trợ leave, ot, trip hoặc equipment."));

            var filter = new HistoryFilterDto
            {
                Kind = requestKind,
                Year = year ?? DateTime.Now.Year,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                SearchText = search,
                Page = page,
                PageSize = pageSize
            };

            var result = await _dispatcher.GetHistoryAsync(filter, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpGet("{kind}/{id:int}")]
        public async Task<IActionResult> GetDetail(string kind, int id, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            if (!TryParseRequestKind(kind, out var requestKind))
                return BadRequest(ApiResponse<object>.Fail("Loại đơn không hợp lệ. Chỉ hỗ trợ leave, ot, trip hoặc equipment."));

            var result = await _dispatcher.GetDetailAsync(requestKind, id, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpGet("{kind}/balance")]
        public async Task<IActionResult> GetBalance(
            string kind,
            [FromQuery] int? year,
            CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            if (!TryParseRequestKind(kind, out var requestKind))
                return BadRequest(ApiResponse<object>.Fail("Loại đơn không hợp lệ. Chỉ hỗ trợ leave, ot, trip hoặc equipment."));

            var result = await _dispatcher.GetBalanceAsync(
                requestKind, year ?? DateTime.Now.Year, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpPost("{kind}/{id:int}/cancel")]
        public async Task<IActionResult> Cancel(
            string kind,
            int id,
            [FromBody] HistoryCancelRequestDto req,
            CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            if (!TryParseRequestKind(kind, out var requestKind))
                return BadRequest(ApiResponse<object>.Fail("Loại đơn không hợp lệ. Chỉ hỗ trợ leave, ot, trip hoặc equipment."));
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            var result = await _dispatcher.CancelAsync(
                requestKind, id, req.Reason, UserInfo, ct);
            return HandleResult(result);
        }

        private static bool TryParseRequestKind(string kind, out RequestModule requestKind)
        {
            return kind.TryToRequestModule(out requestKind);
        }
    }
}

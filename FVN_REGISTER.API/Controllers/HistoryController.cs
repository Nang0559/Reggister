using AutoMapper;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Interfaces.Histories;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            IMapper mapper,
            ILogger<HistoryController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
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

            var requestKind = kind.ToLower() == "ot" ? RequestKind.OT : RequestKind.Leave;

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
            var requestKind = kind.ToLower() == "ot" ? RequestKind.OT : RequestKind.Leave;
            var result = await _dispatcher.GetDetailAsync(requestKind, id, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpGet("{kind}/balance")]
        public async Task<IActionResult> GetBalance(string kind, [FromQuery] int? year, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var requestKind = kind.ToLower() == "ot" ? RequestKind.OT : RequestKind.Leave;
            var result = await _dispatcher.GetBalanceAsync(requestKind, year ?? DateTime.Now.Year, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpPost("{kind}/{id:int}/cancel")]
        public async Task<IActionResult> Cancel(string kind, int id, [FromBody] CancelRequest req, CancellationToken ct)
        {
            if (UserInfo == null) return Unauthorized();
            var requestKind = kind.ToLower() == "ot" ? RequestKind.OT : RequestKind.Leave;
            var result = await _dispatcher.CancelAsync(requestKind, id, req.Reason ?? "", UserInfo, ct);
            return HandleResult(result);
        }
    }

    public record CancelRequest(string? Reason);
}

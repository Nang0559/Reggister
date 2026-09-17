using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Shared.Services.Approvals
{
    public class ApprovalListClientService : IApprovalListClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<ApprovalListClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;
        private bool Debug => _options.CurrentValue.Enabled;

        public ApprovalListClientService(
            IHttpClientWithAuth http,
            ILogger<ApprovalListClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http; _logger = logger; _options = options;
        }

        public Task<ApiResponse<PendingApprovalListDto>> GetPendingAsync(
        CancellationToken ct = default)
        => _http.GetAsync<PendingApprovalListDto>("api/ApprovalList/pending", ct);

        public Task<ApiResponse<object>> ApproveAsync(
            List<int> ids, RequestKind kind, int level,
            string? comment, CancellationToken ct = default)
            => _http.PostAsync<object>("api/ApprovalList/approve",
                new { Ids = ids, Kind = kind, Level = level, Comment = comment }, ct);

        public Task<ApiResponse<object>> RejectAsync(
            List<int> ids, RequestKind kind, int level,
            string comment, CancellationToken ct = default)
            => _http.PostAsync<object>("api/ApprovalList/reject",
                new { Ids = ids, Kind = kind, Level = level, Comment = comment }, ct);
    }
}

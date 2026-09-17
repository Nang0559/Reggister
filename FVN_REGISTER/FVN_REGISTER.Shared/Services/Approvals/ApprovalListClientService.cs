using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Enums;
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
            _http = http;
            _logger = logger;
            _options = options;
        }

        public Task<ApiResponse<List<PendingApprovalGroupDto>>> GetPendingAsync(
            CancellationToken ct = default)
            => _http.GetAsync<List<PendingApprovalGroupDto>>("api/ApprovalList/pending", ct);

        public Task<ApiResponse<object>> ApproveAsync(
            List<int> ids,
            RequestModule kind,
            int level,
            string? comment,
            CancellationToken ct = default)
            => _http.PostAsync<object>("api/ApprovalList/approve",
                new ApprovalActionDto
                {
                    RequestIds = ids,
                    Kind = kind,
                    Level = level,
                    IsReject = false,
                    Comment = comment
                }, ct);

        public Task<ApiResponse<object>> RejectAsync(
            List<int> ids,
            RequestModule kind,
            int level,
            string comment,
            CancellationToken ct = default)
            => _http.PostAsync<object>("api/ApprovalList/reject",
                new ApprovalActionDto
                {
                    RequestIds = ids,
                    Kind = kind,
                    Level = level,
                    IsReject = true,
                    Comment = comment
                }, ct);
    }
}

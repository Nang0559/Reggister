using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Shared.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public class ApproveClientService : IApproveClientService
    {
        private readonly IHttpClientWithAuth _http;

        public ApproveClientService(IHttpClientWithAuth http)
            => _http = http;

        public Task<ApiResponse<List<PendingApprovalGroup>>> GetPendingAsync(
            CancellationToken ct = default)
            => _http.GetAsync<List<PendingApprovalGroup>>("api/LeaveDays/pending", ct);

        public Task<ApiResponse<object>> ApproveAsync(
            List<int> ids, int level, string? comment,
            CancellationToken ct = default)
            => _http.PostAsync<object>("api/LeaveDays/approve",
                new { Ids = ids, Level = level, Comment = comment }, ct);

        public Task<ApiResponse<object>> RejectAsync(
            List<int> ids, int level, string? comment,
            CancellationToken ct = default)
            => _http.PostAsync<object>("api/LeaveDays/reject",
                new { Ids = ids, Level = level, Comment = comment }, ct);
    }
}

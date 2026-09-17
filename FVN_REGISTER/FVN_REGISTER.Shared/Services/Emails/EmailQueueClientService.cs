using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Shared.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Shared.Services.Emails
{
    public class EmailQueueClientService : IEmailQueueClientService
    {
        private readonly IHttpClientWithAuth _http;
        public EmailQueueClientService(IHttpClientWithAuth http) => _http = http;

        public Task<ApiResponse<List<EmailQueueDto>>> GetAllAsync(CancellationToken ct = default)
            => _http.GetAsync<List<EmailQueueDto>>("api/EmailQueue", ct);

        public Task<ApiResponse<object>> RetryAsync(int id, CancellationToken ct = default)
            => _http.PostAsync<object>($"api/EmailQueue/{id}/retry", new { }, ct);

        public Task<ApiResponse<object>> RetryBatchAsync(List<int> ids, CancellationToken ct = default)
            => _http.PostAsync<object>("api/EmailQueue/retry-batch", new { Ids = ids }, ct);

        public Task<ApiResponse<object>> CancelAsync(int id, CancellationToken ct = default)
            => _http.PostAsync<object>($"api/EmailQueue/{id}/cancel", new { }, ct);

        public Task<ApiResponse<object>> CancelBatchAsync(List<int> ids, CancellationToken ct = default)
            => _http.PostAsync<object>("api/EmailQueue/cancel-batch", new { Ids = ids }, ct);

        public Task<ApiResponse<object>> TriggerProcessAsync(CancellationToken ct = default)
            => _http.PostAsync<object>("api/EmailQueue/trigger", new { }, ct);
    }
}

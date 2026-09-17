using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Shared.Services.Emails
{
    public sealed class EmailTemplateClientService : IEmailTemplateClientService
    {
        private const string BaseUrl = "api/EmailTemplate";
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<EmailTemplateClientService> _logger;

        public EmailTemplateClientService(
            IHttpClientWithAuth http,
            ILogger<EmailTemplateClientService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public Task<ApiResponse<List<EmailTemplateDto>>> GetAllAsync(
            CancellationToken ct = default)
            => _http.GetAsync<List<EmailTemplateDto>>(BaseUrl, ct);

        public Task<ApiResponse<EmailTemplateDto>> GetByIdAsync(
            int id,
            CancellationToken ct = default)
            => _http.GetAsync<EmailTemplateDto>($"{BaseUrl}/{id}", ct);

        public Task<ApiResponse<object>> SaveAsync(
            EmailTemplateDto dto,
            CancellationToken ct = default)
            => _http.PostAsync<object>($"{BaseUrl}/save", dto, ct);

        public Task<ApiResponse<object>> ToggleAsync(
            int id,
            CancellationToken ct = default)
            => _http.PostAsync<object>($"{BaseUrl}/{id}/toggle", new { }, ct);
    }
}

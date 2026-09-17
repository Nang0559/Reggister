
using FVN_REGISTER.Contract.Dtos.EmailTemplates;

using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Configurations;

using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.Emails
{
    public class EmailTemplateClientService : IEmailTemplateClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<EmailTemplateClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;
        private bool Debug => _options.CurrentValue.Enabled;

        public EmailTemplateClientService(
            IHttpClientWithAuth http,
            ILogger<EmailTemplateClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        public Task<ApiResponse<List<EmailTemplateDto>>> GetAllAsync(CancellationToken ct = default)
            => _http.GetAsync<List<EmailTemplateDto>>("api/EmailTemplate", ct);

        public Task<ApiResponse<EmailTemplateDto>> GetByIdAsync(int id, CancellationToken ct = default)
            => _http.GetAsync<EmailTemplateDto>($"api/EmailTemplate/{id}", ct);

        public Task<ApiResponse<object>> SaveAsync(EmailTemplateDto dto, CancellationToken ct = default)
            => _http.PostAsync<object>("api/EmailTemplate/save", dto, ct);

        public Task<ApiResponse<object>> ToggleAsync(int id, CancellationToken ct = default)
            => _http.PostAsync<object>($"api/EmailTemplate/{id}/toggle", new { }, ct);
    }
}
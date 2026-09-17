using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using System;


namespace FVN_REGISTER.Shared.Services.Emails
{
    public interface IEmailTemplateClientService
    {
        Task<ApiResponse<List<EmailTemplateDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ApiResponse<EmailTemplateDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> SaveAsync(EmailTemplateDto dto, CancellationToken ct = default);
        Task<ApiResponse<object>> ToggleAsync(int id, CancellationToken ct = default);
    }
}

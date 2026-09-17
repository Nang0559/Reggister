

using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.EmailTemplates
{
    public interface IEmailTemplateManagementService
    {
        Task<List<EmailTemplateDto>> GetAllAsync(CancellationToken ct = default);
        Task<EmailTemplateDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult> SaveAsync(EmailTemplateDto dto, int userId, CancellationToken ct = default);
        Task<ServiceResult> ToggleActiveAsync(int id, int userId, CancellationToken ct = default);
    }
}

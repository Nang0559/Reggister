using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Emails;

public interface IEmailAdminService
{
    Task<List<EmailProfileDto>> GetProfilesAsync(CancellationToken ct = default);
    Task<EmailProfileDto?> GetProfileAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> SaveProfileAsync(EmailProfileDto dto, int userId, CancellationToken ct = default);
    Task<ServiceResult> TestProfileAsync(int id, string toEmail, CancellationToken ct = default);

    Task<List<EmailDispatchPolicyDto>> GetPoliciesAsync(CancellationToken ct = default);
    Task<ServiceResult> SavePolicyAsync(EmailDispatchPolicyDto dto, int userId, CancellationToken ct = default);
    Task<ServiceResult> TogglePolicyAsync(int id, int userId, CancellationToken ct = default);
}

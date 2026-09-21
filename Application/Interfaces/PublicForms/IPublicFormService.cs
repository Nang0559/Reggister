using FVN_REGISTER.Contract.Dtos.PublicForms;
using FVN_REGISTER.Contract.Requests.PublicForms;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.PublicForms;

public interface IPublicFormService
{
    Task<List<PublicFormDto>> GetManageListAsync(CancellationToken ct = default);
    Task<List<PublicFormDto>> GetAvailableAsync(string employeeCode, string? deptCode, string? positionCode, CancellationToken ct = default);
    Task<PublicFormDto?> GetAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<PublicFormDto>> CreateAsync(SavePublicFormRequest request, int actorUserId, CancellationToken ct = default);
    Task<ServiceResult<PublicFormDto>> UpdateAsync(int id, SavePublicFormRequest request, int actorUserId, CancellationToken ct = default);
    Task<ServiceResult> PublishAsync(int id, int actorUserId, CancellationToken ct = default);
    Task<ServiceResult> CloseAsync(int id, int actorUserId, CancellationToken ct = default);
    Task<ServiceResult<int>> SubmitAsync(int formId, string employeeCode, string? deptCode, string? positionCode, IReadOnlyCollection<PublicFormAnswerRequest> answers, CancellationToken ct = default);
}

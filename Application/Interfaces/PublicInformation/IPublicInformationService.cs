
using FVN_REGISTER.Contract.Dtos.PublicInformation;
using FVN_REGISTER.Contract.Requests.PublicInformation;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.PublicInformation;

public interface IPublicInformationService
{
    Task<List<PublicInformationDto>> GetPublishedAsync(CancellationToken ct = default);
    Task<List<PublicInformationDto>> GetManageListAsync(CancellationToken ct = default);
    Task<PublicInformationDto?> GetAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<PublicInformationDto>> CreateAsync(SavePublicInformationRequest request, int actorUserId, CancellationToken ct = default);
    Task<ServiceResult<PublicInformationDto>> UpdateAsync(int id, SavePublicInformationRequest request, int actorUserId, CancellationToken ct = default);
    Task<ServiceResult> PublishAsync(int id, int actorUserId, CancellationToken ct = default);
    Task<ServiceResult> ArchiveAsync(int id, int actorUserId, CancellationToken ct = default);
}

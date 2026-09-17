using FVN_REGISTER.Contract.Dtos.OtReasons;
using FVN_REGISTER.Core.Utils;


namespace FVN_REGISTER.Application.Interfaces.OTReasonCodes
{
    public interface IOTReasonCodeManagementService
    {
        Task<ServiceResult<List<OTReasonCodeDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<List<OTReasonCodeDto>>> GetFilteredAsync(bool? isActive, CancellationToken ct = default);
        Task<ServiceResult<OTReasonCodeDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult> CreateAsync(OTReasonCodeUpsertDto model, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> UpdateAsync(OTReasonCodeUpsertDto model, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
        Task<ServiceResult> ToggleActiveAsync(int id, int currentUserId, CancellationToken ct = default);
    }
}

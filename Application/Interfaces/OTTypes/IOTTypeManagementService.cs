using FVN_REGISTER.Contract.Dtos.OTTypeDtos;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Utils;


namespace FVN_REGISTER.Application.Interfaces.OTTypes
{
    public interface IOTTypeManagementService
    {
        Task<ServiceResult<List<OTTypeDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<List<OTTypeDto>>> GetFilteredAsync(bool? isActive, CancellationToken ct = default);
        Task<ServiceResult<OTTypeDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult> CreateAsync(OTTypeUpsertDto model, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> UpdateAsync(OTTypeUpsertDto model, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
        Task<ServiceResult> ToggleActiveAsync(int id, int currentUserId, CancellationToken ct = default);
    }
}

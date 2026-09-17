

using FVN_REGISTER.Contract.Dtos.LeaveTypes;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Leaves
{
    public interface ILeaveTypeManagementService
    {
        Task<ServiceResult<List<LeaveTypeDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<List<LeaveTypeDto>>> GetFilteredAsync(bool? tinhPhep, bool? isActive, CancellationToken ct = default);
        Task<ServiceResult> CreateAsync(LeaveTypeUpsertDto model, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> UpdateAsync(LeaveTypeUpsertDto model, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> ToggleAsync(int id, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
        // KHÔNG còn SyncFromHrmAsync — xem LeaveTypeStagingImporter + LeaveTypeHrmSyncJob
    }
}

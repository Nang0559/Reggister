using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.HrmSync;

public interface IHrmSyncService
{
    Task<ServiceResult<HrmSyncRunResultDto>> RunAllAsync(
        string? triggeredBy = null,
        bool manual = true,
        CancellationToken ct = default);

    Task<ServiceResult<HrmSyncRunResultDto>> RunEntityAsync(
        string entityType,
        string? triggeredBy = null,
        CancellationToken ct = default);

    HrmSyncRuntimeStatusDto GetRuntimeStatus();
}

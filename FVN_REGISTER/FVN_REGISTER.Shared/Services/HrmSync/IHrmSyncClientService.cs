using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.HrmSync;

public interface IHrmSyncClientService
{
    Task<ApiResponse<HrmSyncRuntimeStatusDto>> GetStatusAsync(CancellationToken ct = default);
    Task<ApiResponse<HrmSyncRunResultDto>> RunAllAsync(CancellationToken ct = default);
    Task<ApiResponse<HrmSyncRunResultDto>> RunEntityAsync(string entityType, CancellationToken ct = default);
    Task<ApiResponse<HrmSyncRunResultDto>> ReconcileSecurityAsync(CancellationToken ct = default);
}

using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.HrmSync;

public interface IHrmSyncReviewClientService
{
    Task<ApiResponse<List<SyncReviewFlagDto>>> GetUnresolvedAsync(string? entityType = null, CancellationToken ct = default);
    Task<ApiResponse<int>> CountAsync(CancellationToken ct = default);
    Task<ApiResponse<object>> ResolveAsync(int flagId, CancellationToken ct = default);
    Task<ApiResponse<object>> ResolveByEntityAsync(string entityType, string entityKey, CancellationToken ct = default);
}

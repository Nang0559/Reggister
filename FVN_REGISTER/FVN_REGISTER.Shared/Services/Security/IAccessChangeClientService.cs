using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Shared.Services.Security;

public interface IAccessChangeClientService
{
    Task<ApiResponse<List<AccessChangeEmployeeOptionDto>>> GetEmployeesAsync(CancellationToken ct = default);
    Task<ApiResponse<List<AccessChangeFunctionOptionDto>>> GetFunctionOptionsAsync(RequestModule module, string oldEmployeeCode, CancellationToken ct = default);
    Task<ApiResponse<List<FVN_REGISTER.Contract.Dtos.Equipment.EquipmentHandoverCandidateDto>>> GetEquipmentCandidatesAsync(string oldEmployeeCode, CancellationToken ct = default);
    Task<ApiResponse<AccessChangeRequestDto>> CreateAsync(AccessChangeRequestCreateDto request, CancellationToken ct = default);
    Task<ApiResponse<List<AccessChangeRequestDto>>> GetPendingAsync(CancellationToken ct = default);
    Task<ApiResponse<List<AccessChangeRequestDto>>> GetMineAsync(CancellationToken ct = default);
    Task<ApiResponse<AccessChangeRequestDto>> GetAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<AccessChangeRequestDto>> ApproveAsync(int id, int level, string? comment, CancellationToken ct = default);
    Task<ApiResponse<AccessChangeRequestDto>> RejectAsync(int id, int level, string comment, CancellationToken ct = default);
    Task<ApiResponse<List<AccessChangeItQueueItemDto>>> GetItQueueAsync(CancellationToken ct = default);
    Task<ApiResponse<AccessChangeRequestDto>> ExecuteByItAsync(int id, AccessChangeItExecuteDto request, CancellationToken ct = default);
}

using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Security;

public interface IAccessChangeService
{
    Task<ServiceResult<List<AccessChangeEmployeeOptionDto>>> GetEmployeeOptionsAsync(CancellationToken ct = default);
    Task<ServiceResult<List<AccessChangeFunctionOptionDto>>> GetFunctionOptionsAsync(RequestModule businessModule, string oldEmployeeCode, CancellationToken ct = default);
    Task<ServiceResult<AccessChangeRequestDto>> CreateAndSubmitAsync(AccessChangeRequestCreateDto request, CancellationToken ct = default);
    Task<ServiceResult<AccessChangeRequestDto>> GetAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<List<AccessChangeRequestDto>>> GetMineAsync(CancellationToken ct = default);
    Task<ServiceResult<List<AccessChangeRequestDto>>> GetPendingApprovalsAsync(CancellationToken ct = default);
    Task<ServiceResult<AccessChangeRequestDto>> ApproveAsync(int id, int level, string? comment, CancellationToken ct = default);
    Task<ServiceResult<AccessChangeRequestDto>> RejectAsync(int id, int level, string comment, CancellationToken ct = default);
    Task<ServiceResult<List<AccessChangeItQueueItemDto>>> GetItQueueAsync(CancellationToken ct = default);
    Task<ServiceResult<AccessChangeRequestDto>> ExecuteByItAsync(int id, AccessChangeItExecuteDto request, CancellationToken ct = default);
}

using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Leaves;

public interface ILeaveEntitlementClientService
{
    Task<ApiResponse<LeaveEntitlementDto>> GetMineAsync(
        int? year = null,
        CancellationToken ct = default);
}

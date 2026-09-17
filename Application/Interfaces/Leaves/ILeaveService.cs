using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Leaves
{
    public interface ILeaveService : IRequestCommandService<LeaveRequestUpsertDto>
    {
        Task<ServiceResult<int>> UpdateAsync(
            LeaveRequestUpsertDto model,
            UserIdentityDto user,
            CancellationToken ct = default);

        Task<ServiceResult> CancelDetailAsync(
            int detailId,
            string reason,
            UserIdentityDto user,
            CancellationToken ct = default);
    }
}

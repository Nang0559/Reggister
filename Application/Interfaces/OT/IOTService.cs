using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.OT
{
    public interface IOTService : IRequestCommandService<OTRequestUpsertDto>
    {
        Task<ServiceResult> JoinAsync(
            int otRequestId, UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult> RemoveEmployeeAsync(
            int otRequestId, string employeeCode, UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult> UpdateEmployeeOTInfoAsync(
            int otRequestId, List<OTEmployeeDto> updates, UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult> AddEmployeesAsync(
            int otRequestId, List<OTEmployeeDto> newEmployees, UserIdentityDto user, CancellationToken ct = default);
    }
}

using FVN_REGISTER.Contract.Dtos.Leaves;

namespace FVN_REGISTER.Application.Interfaces.Leaves;

public interface ILeaveEntitlementService
{
    Task<LeaveEntitlementDto> EnsureCalculatedAsync(
        string employeeCode,
        int workYear,
        CancellationToken ct = default);
}

using FVN_REGISTER.Contract.Dtos.Leaves;

namespace FVN_REGISTER.Application.Interfaces.Leaves;

public interface ILeaveEntitlementService
{
    Task<LeaveEntitlementDto> EnsureCalculatedAsync(
        string employeeCode,
        int workYear,
        CancellationToken ct = default);

    /// <summary>
    /// Tự động tính/cập nhật entitlement cho toàn bộ nhân viên đang active của năm làm việc.
    /// Idempotent: chỉ ghi lại các balance thiếu hoặc đã cũ trong ngày.
    /// </summary>
    Task EnsureWorkYearCalculatedAsync(
        int workYear,
        CancellationToken ct = default);
}

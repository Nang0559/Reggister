


using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Orchestrators
{
    // <summary>
    /// Điều phối toàn bộ vòng đời của 1 đơn nghỉ phép:
    /// Validate -> Ghi dữ liệu (ILeaveService) -> Khởi tạo/hủy luồng duyệt (IApprovalWorkflowOrchestrator)
    /// -> Bắn thông báo. Các Controller chỉ nên gọi vào đây, không gọi thẳng ILeaveService.
    /// </summary>
    public interface ILeaveOrchestrator
    {
        Task<ServiceResult<LeaveRequestDto>> CreateAsync(
            LeaveRequestUpsertDto model, UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult<LeaveRequestDto>> UpdateAsync(
            LeaveRequestUpsertDto model, UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult> CancelAsync(
            int leaveId, string reason, UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult<LeaveRequestDto>> GetDetailsAsync(
            int leaveId, CancellationToken ct = default);

        // SỬA: BalanceSummaryDto -> LeaveBalanceDto (khớp LeaveQueryService.GetSimpleBalanceAsync)
        Task<ServiceResult<LeaveBalanceDto>> GetBalanceSummaryAsync(
            string employeeCode, int year, CancellationToken ct = default);

        // SỬA: tách làm 2 hàm thay vì 1 GetHistoryAsync(filter) không khớp thực tế —
        // 1 cho cá nhân (không filter), 1 cho danh sách có filter/phân trang
        Task<ServiceResult<List<LeaveSummaryDto>>> GetRecentHistoryAsync(
            string employeeCode, int limit = 5, CancellationToken ct = default);

        Task<ServiceResult<PaginationResult<LeaveSummaryDto>>> GetPagedHistoryAsync(
            string? deptCode, ApprovalStatus? status, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default);
    }
}

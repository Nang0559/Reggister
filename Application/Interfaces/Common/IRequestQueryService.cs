using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;



namespace FVN_REGISTER.Application.Interfaces.Common
{
    // Hợp đồng CHUNG cho mọi module request (Leave, OT, Trip...).
    // TSummary: DTO nhẹ (dashboard, danh sách, phân trang)
    // TBalance: DTO số dư (phép còn lại / giờ OT...)
    // TDto: DTO đầy đủ (chi tiết 1 đơn, xem theo ngày/phòng ban)
    public interface IRequestQueryService<TSummary, TBalance, TDto>
        where TSummary : class
        where TDto : class
    {
        /// Widget cá nhân — đơn của TÔI, không trộn số liệu phòng ban/công ty.
        Task<List<WidgetCounterDto>> GetMyWidgetsAsync(string employeeCode, CancellationToken ct = default);

        /// N đơn gần nhất — DTO nhẹ, dùng cho Dashboard/Inbox.
        Task<List<TSummary>> GetRecentSummaryAsync(string employeeCode, int limit = 5, CancellationToken ct = default);

        /// Số dư hiện tại (phép còn lại / giờ OT tháng này...), dùng cho Dashboard — theo NĂM hiện tại, không cần chọn tháng.
        Task<TBalance> GetSimpleBalanceAsync(string employeeCode, int year, CancellationToken ct = default);

        /// Danh sách đầy đủ có phân trang, dùng cho trang lịch sử/danh sách.
        Task<PaginationResult<TSummary>> GetPagedAsync(
            string? deptCode, ApprovalStatus? status, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default);

        /// Chi tiết đầy đủ 1 đơn — kèm Details + ApprovalSteps (đã có sẵn qua BaseRequestQueryService).
        Task<ServiceResult<TDto>> GetFullDetailsAsync(int requestId, CancellationToken ct = default);

        /// Danh sách đầy đủ của cả PHÒNG BAN trong 1 ngày cụ thể — vd: "ai đang nghỉ/OT hôm nay".
        Task<List<TDto>> GetDeptByDateAsync(string deptCode, DateTime date, CancellationToken ct = default);
    }
}

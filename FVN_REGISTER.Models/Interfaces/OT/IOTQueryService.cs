using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;

namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTQueryService
    {
        // Dữ liệu tổng hợp để render trang tạo đơn OT
        Task<CombinedOTViewModel> GetCombinedDataAsync(
            string employeeCode,
            string deptCode,
            string cvCode,
            int year,
            CancellationToken ct = default);

        // Số lượng đơn chờ duyệt theo từng level (dùng cho badge/widget)
        Task<List<OTPendingGroup>> GetPendingSummaryAsync(
            string approverEmail,
            CancellationToken ct = default);

        // Danh sách đơn chờ duyệt kèm chi tiết (dùng cho trang Approve)
        Task<List<OTPendingGroup>> GetPendingDetailsAsync(
            string approverEmail,
            CancellationToken ct = default);

        // Lịch sử N đơn OT gần nhất của nhân viên
        Task<List<OTRequestViewModel>> GetRecentHistoryAsync(
            string employeeCode,
            int limit,
            CancellationToken ct = default);

        // Validate giới hạn giờ OT theo từng nhân viên trong model
        Task<OTValidationResultDto> ValidateHoursAsync(
            CreateOTRequestModel model,
            CancellationToken ct = default);

        // Lấy danh sách approver theo level + phòng ban
        Task<List<F03OTApprover>> GetApproversAsync(
            int level,
            string deptCode,
            CancellationToken ct = default);

        // Widget counters cho Dashboard
        Task<List<WidgetCounterDto>> GetDashboardWidgetsAsync(
            string approverEmail,
            string deptCode,
            CancellationToken ct = default);
        Task<PaginationResult<OTRequestViewModel>> GetPagedAsync(
        string? deptCode,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
    CancellationToken ct = default);

        // Số dư giờ OT của một nhân viên
        Task<OTBalanceDto> GetBalanceAsync(
            string employeeCode,
            int year,
            int month,
            CancellationToken ct = default);
    }
}
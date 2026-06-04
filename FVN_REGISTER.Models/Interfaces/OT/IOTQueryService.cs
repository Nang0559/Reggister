using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.ViewModels.OT;





    namespace FVN_REGISTER.Contract.Interfaces.OT
    {
    public interface IOTQueryService
    {
        /// <summary>
        /// Trả về toàn bộ dữ liệu cần thiết để render trang tạo / sửa đơn OT:
        /// giới hạn giờ, số dư, danh sách approver, danh sách nhân viên phòng ban.
        /// </summary>
        Task<CombinedOTViewModel> GetCombinedDataAsync(
            string employeeCode,
            string deptCode,
            string cvCode,
            int year,
            CancellationToken ct = default);

        /// <summary>Danh sách đơn OT chờ duyệt theo email người duyệt (tóm tắt).</summary>
        Task<List<OTPendingGroup>> GetPendingSummaryAsync(
            string approverEmail,
            CancellationToken ct = default);

        /// <summary>Danh sách đơn OT chờ duyệt kèm chi tiết request.</summary>
        Task<List<OTPendingGroup>> GetPendingDetailsAsync(
            string approverEmail,
            CancellationToken ct = default);

        /// <summary>Lịch sử đơn OT gần đây của nhân viên.</summary>
        Task<List<OTRequestViewModel>> GetRecentHistoryAsync(
            string employeeCode,
            int limit,
            CancellationToken ct = default);

        /// <summary>Validate giờ OT trước khi lưu (daily / monthly / yearly).</summary>
        Task<OTValidationResultDto> ValidateHoursAsync(
            CreateOTRequestModel model,
            CancellationToken ct = default);

        /// <summary>Danh sách approver theo level và phòng ban.</summary>
        Task<List<F03OTApprover>> GetApproversAsync(
            int level,
            string deptCode,
            CancellationToken ct = default);

        /// <summary>Widget counter cho Dashboard OT (chờ duyệt, tổng giờ OT hôm nay...).</summary>
        Task<List<WidgetCounterDto>> GetDashboardWidgetsAsync(
            string approverEmail,
            string deptCode,
            CancellationToken ct = default);
    }
}


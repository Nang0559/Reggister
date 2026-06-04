using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels.OT;




    namespace FVN_REGISTER.Contract.Interfaces.OT
    {
        public interface IOTQueryService
        {
            // ================= DÀNH CHO NHÂN VIÊN =================

            /// <summary>Lấy dữ liệu tổng hợp để hiển thị trang tạo đơn OT</summary>
            Task<CreateOTRequestModel> GetCombinedDataAsync(
                string empCode,
                string deptCode,
                string cvCode,
                CancellationToken ct = default);

            /// <summary>Lấy số dư giờ OT của nhân viên (đã dùng / còn lại theo loại)</summary>
            Task<OTBalanceDto> GetOTBalanceAsync(
                string employeeCode,
                int year,
                CancellationToken ct = default);

            /// <summary>Lấy lịch sử đơn OT gần đây</summary>
            Task<List<OTRequestViewModel>> GetRecentOTRequestsAsync(
                string employeeCode,
                int limit = 10,
                CancellationToken ct = default);

            /// <summary>Lấy chi tiết 1 đơn OT</summary>
            Task<OTRequestViewModel?> GetOTDetailAsync(
                int otRequestId,
                CancellationToken ct = default);

            // ================= DÀNH CHO APPROVER =================

            /// <summary>Lấy danh sách đơn đang chờ duyệt theo email approver</summary>
            Task<List<OTRequestViewModel>> GetPendingApprovalsAsync(
                string approverEmail,
                int level,
                CancellationToken ct = default);

            /// <summary>Đếm số đơn chờ duyệt (dùng cho widget dashboard)</summary>
            Task<int> CountPendingApprovalsAsync(
                string approverEmail,
                CancellationToken ct = default);

            // ================= DÀNH CHO HR / ADMIN =================

            /// <summary>Lấy danh sách đơn OT có phân trang và lọc</summary>
            Task<PaginationResult<OTRequestViewModel>> GetPagedOTRequestsAsync(
                string? deptCode,
                string? status,
                DateTime? fromDate,
                DateTime? toDate,
                int page,
                int pageSize,
                CancellationToken ct = default);

            /// <summary>Lấy dữ liệu dashboard OT (widget counts)</summary>
            Task<OTDashboardViewModel> GetDashboardAsync(
                string employeeCode,
                string deptCode,
                int? permissionCode,
                CancellationToken ct = default);

            /// <summary>Kiểm tra validate giờ OT trước khi submit (gọi từ UI real-time)</summary>
            Task<OTValidationResultDto> ValidateOTHoursAsync(
                string employeeCode,
                DateTime otDate,
                decimal hours,
                string otType,
                CancellationToken ct = default);

            /// <summary>Lấy danh sách approver theo phòng ban và cấp</summary>
            Task<List<OTApprovalStep>> GetApproversAsync(
                string deptCode,
                string cvCode,
                CancellationToken ct = default);
        }
    }


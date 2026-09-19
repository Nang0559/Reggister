


using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Orchestrators
{
    /// <summary>
    /// Điều phối toàn bộ vòng đời của 1 đơn tăng ca (OT):
    /// Validate -> Ghi dữ liệu (IOTService) -> Khởi tạo/hủy luồng duyệt (IApprovalWorkflowOrchestrator).
    /// Dữ liệu chấm công/OT thực tế do HRM attendance calculation pipeline xử lý tập trung.
    /// </summary>
    public interface IOTOrchestrator
    {
        Task<ServiceResult<OTRequestDto>> CreateAsync(
            OTRequestUpsertDto model, UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult<OTRequestDto>> UpdateAsync(
            OTRequestUpsertDto model, UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult> CancelAsync(
            int otRequestId, string reason, UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult<OTRequestDto>> GetDetailsAsync(
            int otRequestId, CancellationToken ct = default);

        Task<ServiceResult<OTBalanceDto>> GetBalanceSummaryAsync(
            string employeeCode, int year, CancellationToken ct = default);

        Task<ServiceResult<List<OTSummaryDto>>> GetRecentHistoryAsync(
            string employeeCode, int limit = 5, CancellationToken ct = default);

        Task<ServiceResult<PaginationResult<OTSummaryDto>>> GetPagedHistoryAsync(
            string? deptCode, ApprovalStatus? status, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default);
    }
}

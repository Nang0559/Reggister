using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Application.Orchestrators
{// <summary>
    /// Orchestrator cho nghiệp vụ Nghỉ phép.
    /// Nguyên tắc: KHÔNG đụng DbContext trực tiếp. Chỉ điều phối:
    ///   1) ILeaveValidator                              - kiểm tra hợp lệ nghiệp vụ
    ///   2) ILeaveService                                 - đọc/ghi thực thể LeaveRequest
    ///   3) ILeaveQueryService                             - query tổng hợp phục vụ UI (balance, history...)
    ///   4) IApprovalWorkflowOrchestrator&lt;F03LeaveDay&gt; - khởi tạo luồng duyệt (InitApprovalAsync)
    ///
    /// Approve/Reject/Pending-list KHÔNG nằm ở đây — thuộc về orchestrator/controller Approval dùng chung
    /// (gọi trực tiếp IApprovalWorkflowOrchestrator&lt;TSubject&gt;.ApproveAsync/RejectAsync/GetPendingForApproverAsync).
    /// </summary>
    /// <summary>
    /// Orchestrator cho nghiệp vụ Nghỉ phép.
    /// Nguyên tắc: KHÔNG đụng DbContext trực tiếp. Chỉ điều phối:
    ///   1) ILeaveValidator                              - kiểm tra hợp lệ nghiệp vụ
    ///   2) ILeaveService                                 - đọc/ghi thực thể LeaveRequest
    ///   3) ILeaveQueryService                             - query tổng hợp phục vụ UI (balance, history...)
    ///   4) IApprovalWorkflowOrchestrator&lt;LeaveRequestSubject&gt; - khởi tạo luồng duyệt (InitApprovalAsync)
    ///
    /// Approve/Reject/Pending-list KHÔNG nằm ở đây — thuộc về orchestrator/controller Approval dùng chung
    /// (gọi trực tiếp IApprovalWorkflowOrchestrator&lt;TSubject&gt;.ApproveAsync/RejectAsync/GetPendingForApproverAsync).
    /// </summary>
    public class LeaveOrchestrator : BaseService<LeaveOrchestrator>, ILeaveOrchestrator
    {
        private readonly ILeaveService _leaveService;
        private readonly ILeaveQueryService _query;
        private readonly ILeaveValidator _validator;
        

        public LeaveOrchestrator(
            ILeaveService leaveService,
            ILeaveQueryService query,
            ILeaveValidator validator,
           
            ILogger<LeaveOrchestrator> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _leaveService = leaveService;
            _query = query;
            _validator = validator;
        
        }

        // ================= CREATE =================
        public async Task<ServiceResult<LeaveRequestDto>> CreateAsync(
            LeaveRequestUpsertDto model,
            UserIdentityDto user,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[ORCH][LEAVE] Create start: {User}", user.UserName);

                var valResult = await _validator.ValidateAsync(model, user, ct);
                if (!valResult.IsSuccess)
                {
                    Logger.LogWarnIf(Debug, "[ORCH][LEAVE] Validation failed: {User} - {Msg}", user.UserName, valResult.Message);
                    return ServiceResult<LeaveRequestDto>.Fail(valResult.Message ?? "Dữ liệu không hợp lệ.");
                }

                // LeaveService.CreateAsync trả ServiceResult<int> (Id vừa tạo) — KHÔNG phải DTO đầy đủ
                var createResult = await _leaveService.CreateAsync(model, user, ct);
                if (!createResult.IsSuccess)
                {
                    Logger.LogWarnIf(Debug, "[ORCH][LEAVE] Create entity failed: {User} - {Msg}", user.UserName, createResult.Message);
                    return ServiceResult<LeaveRequestDto>.Fail(createResult.Message ?? "Không thể tạo đơn nghỉ.");
                }

                var leaveId = createResult.Data;

                // SỬA: dùng đúng GetFullDetailsAsync (có sẵn từ BaseRequestQueryService),
                // không có GetDetailsAsync riêng
                var fullResult = await _query.GetFullDetailsAsync(leaveId, ct);
                if (!fullResult.IsSuccess || fullResult.Data == null)
                {
                    Logger.LogWarnIf(Debug, "[ORCH][LEAVE] Cannot reload after create: LeaveId={LeaveId}", leaveId);
                    return ServiceResult<LeaveRequestDto>.Fail("Đơn đã tạo nhưng không tải lại được chi tiết.");
                }

                Logger.LogInfoIf(Debug, "[ORCH][LEAVE] Created & workflow started: LeaveId={LeaveId}", leaveId);

                return ServiceResult<LeaveRequestDto>.Ok(fullResult.Data, "Đã gửi đơn nghỉ thành công.");
            }
            catch (Exception ex)
            {
                return InternalError<LeaveRequestDto>(ex, "Lỗi hệ thống khi tạo đơn nghỉ.");
            }
        }

        public async Task<ServiceResult<LeaveRequestDto>> UpdateAsync(
    LeaveRequestUpsertDto model,
    UserIdentityDto user,
    CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[ORCH][LEAVE] Update start: Id={Id} User={User}", model.Id, user.EmployeeCode);

                var valResult = await _validator.ValidateAsync(model, user, ct);
                if (!valResult.IsSuccess)
                    return ServiceResult<LeaveRequestDto>.Fail(valResult.Message ?? "Dữ liệu không hợp lệ.");

                var updateResult = await _leaveService.UpdateAsync(model, user, ct);
                if (!updateResult.IsSuccess)
                    return ServiceResult<LeaveRequestDto>.Fail(updateResult.Message ?? "Không thể cập nhật đơn nghỉ.");

                var leaveId = updateResult.Data;   // SỬA: dùng Data thay vì model.Id, đồng bộ với CreateAsync

                var fullResult = await _query.GetFullDetailsAsync(leaveId, ct);
                if (!fullResult.IsSuccess || fullResult.Data == null)
                    return ServiceResult<LeaveRequestDto>.Fail("Đã cập nhật nhưng không tải lại được chi tiết.");

                Logger.LogInfoIf(Debug, "[ORCH][LEAVE] Updated: Id={Id}", leaveId);
                return ServiceResult<LeaveRequestDto>.Ok(fullResult.Data);
            }
            catch (Exception ex)
            {
                return InternalError<LeaveRequestDto>(ex, "Lỗi hệ thống khi cập nhật đơn nghỉ.");
            }
        }

        public async Task<ServiceResult> CancelAsync(
        int leaveId, string reason, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[ORCH][LEAVE] Cancel start: LeaveId={Id} User={User}",
                    leaveId, user.EmployeeCode);

                var result = await _leaveService.CancelAsync(leaveId, reason, user, ct);

                if (!result.IsSuccess)
                    Logger.LogWarnIf(Debug, "[ORCH][LEAVE] Cancel failed: LeaveId={Id} - {Msg}", leaveId, result.Message);
                else
                    Logger.LogInfoIf(Debug, "[ORCH][LEAVE] Cancelled: LeaveId={Id}", leaveId);

                return result;
            }
            catch (Exception ex)
            {
                return InternalError(ex, "Lỗi hệ thống khi hủy đơn nghỉ.");
            }
        }

        // ================= READ =================
        public async Task<ServiceResult<LeaveRequestDto>> GetDetailsAsync(int leaveId, CancellationToken ct = default)
        {
            try
            {
                // SỬA: GetFullDetailsAsync đã trả sẵn ServiceResult<TDto>, không cần bọc lại thủ công
                return await _query.GetFullDetailsAsync(leaveId, ct);
            }
            catch (Exception ex)
            {
                return InternalError<LeaveRequestDto>(ex, "Lỗi hệ thống khi lấy chi tiết đơn nghỉ.");
            }
        }

        public async Task<ServiceResult<LeaveBalanceDto>> GetBalanceSummaryAsync(
            string employeeCode,
            int year,
            CancellationToken ct = default)
        {
            try
            {
                // SỬA: đúng tên hàm thật GetSimpleBalanceAsync, đúng type LeaveBalanceDto
                var data = await _query.GetSimpleBalanceAsync(employeeCode, year, ct);
                return ServiceResult<LeaveBalanceDto>.Ok(data);
            }
            catch (Exception ex)
            {
                return InternalError<LeaveBalanceDto>(ex, "Lỗi hệ thống khi lấy số dư phép.");
            }
        }

        // SỬA: đổi tên/chữ ký để khớp hàm thật của LeaveQueryService (không có GetHistoryAsync(filter))
        public async Task<ServiceResult<List<LeaveSummaryDto>>> GetRecentHistoryAsync(
            string employeeCode,
            int limit = 5,
            CancellationToken ct = default)
        {
            try
            {
                var data = await _query.GetRecentSummaryAsync(employeeCode, limit, ct);
                return ServiceResult<List<LeaveSummaryDto>>.Ok(data);
            }
            catch (Exception ex)
            {
                return InternalError<List<LeaveSummaryDto>>(ex, "Lỗi hệ thống khi lấy lịch sử nghỉ phép.");
            }
        }

        // BỔ SUNG: nếu UI cần lịch sử có filter + phân trang (thay cho HistoryFilterDto không tồn tại)
        public async Task<ServiceResult<PaginationResult<LeaveSummaryDto>>> GetPagedHistoryAsync(
            string? deptCode, ApprovalStatus? status, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default)
        {
            try
            {
                var data = await _query.GetPagedAsync(deptCode, status, fromDate, toDate, page, pageSize, ct);
                return ServiceResult<PaginationResult<LeaveSummaryDto>>.Ok(data);
            }
            catch (Exception ex)
            {
                return InternalError<PaginationResult<LeaveSummaryDto>>(ex, "Lỗi hệ thống khi lấy danh sách đơn nghỉ.");
            }
        }
    }
}

using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Application.Orchestrators
{
    /// <summary>
    /// Orchestrator cho nghiệp vụ Nghỉ phép.
    /// Không truy cập DbContext hoặc Infrastructure trực tiếp; chỉ điều phối
    /// các application ports (validator, service, query và approval workflow).
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
                    return ServiceResult<LeaveRequestDto>.Fail(valResult.Message ?? "Dữ liệu không hợp lệ.");

                var createResult = await _leaveService.CreateAsync(model, user, ct);
                if (!createResult.IsSuccess)
                    return ServiceResult<LeaveRequestDto>.Fail(createResult.Message ?? "Không thể tạo đơn nghỉ.");

                var leaveId = createResult.Data;
                var fullResult = await _query.GetFullDetailsAsync(leaveId, ct);
                if (!fullResult.IsSuccess || fullResult.Data == null)
                    return ServiceResult<LeaveRequestDto>.Fail("Đơn đã tạo nhưng không tải lại được chi tiết.");

                Logger.LogInfoIf(Debug, "[ORCH][LEAVE] Created: LeaveId={LeaveId}", leaveId);
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

                var leaveId = updateResult.Data;
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
                Logger.LogDebugIf(Debug, "[ORCH][LEAVE] Cancel start: LeaveId={Id} User={User}", leaveId, user.EmployeeCode);
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

        public async Task<ServiceResult<LeaveRequestDto>> GetDetailsAsync(int leaveId, CancellationToken ct = default)
        {
            try
            {
                return await _query.GetFullDetailsAsync(leaveId, ct);
            }
            catch (Exception ex)
            {
                return InternalError<LeaveRequestDto>(ex, "Lỗi hệ thống khi lấy chi tiết đơn nghỉ.");
            }
        }

        public async Task<ServiceResult<LeaveBalanceDto>> GetBalanceSummaryAsync(
            string employeeCode, int year, CancellationToken ct = default)
        {
            try
            {
                var data = await _query.GetSimpleBalanceAsync(employeeCode, year, ct);
                return ServiceResult<LeaveBalanceDto>.Ok(data);
            }
            catch (Exception ex)
            {
                return InternalError<LeaveBalanceDto>(ex, "Lỗi hệ thống khi lấy số dư phép.");
            }
        }

        public async Task<ServiceResult<List<LeaveSummaryDto>>> GetRecentHistoryAsync(
            string employeeCode, int limit = 5, CancellationToken ct = default)
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

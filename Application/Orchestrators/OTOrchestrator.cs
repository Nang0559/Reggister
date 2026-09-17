using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Application.Orchestrators
{
    /// <summary>
    /// Orchestrator cho nghiệp vụ Tăng ca (OT).
    /// Chỉ điều phối application ports; không truy cập DbContext/EF/Infrastructure.
    /// </summary>
    public class OTOrchestrator : BaseService<OTOrchestrator>, IOTOrchestrator
    {
        private readonly IOTService _otService;
        private readonly IOTQueryService _query;
        private readonly IOTValidator _validator;
        private readonly IOTAttendanceReconciliationService _reconciliation;

        public OTOrchestrator(
            IOTService otService,
            IOTQueryService query,
            IOTValidator validator,
            IOTAttendanceReconciliationService reconciliation,
            ILogger<OTOrchestrator> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _otService = otService;
            _query = query;
            _validator = validator;
            _reconciliation = reconciliation;
        }

        public async Task<ServiceResult<OTRequestDto>> CreateAsync(
            OTRequestUpsertDto model, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[ORCH][OT] Create start: {User}", user.UserName);

                var valResult = await _validator.ValidateCreateAsync(model, user, ct);
                if (!valResult.IsSuccess)
                    return ServiceResult<OTRequestDto>.Fail(valResult.Message ?? "Dữ liệu tăng ca không hợp lệ.");

                var createResult = await _otService.CreateAsync(model, user, ct);
                if (!createResult.IsSuccess)
                    return ServiceResult<OTRequestDto>.Fail(createResult.Message ?? "Không thể tạo đơn tăng ca.");

                var otRequestId = createResult.Data;
                var fullResult = await _query.GetFullDetailsAsync(otRequestId, ct);
                if (!fullResult.IsSuccess || fullResult.Data == null)
                    return ServiceResult<OTRequestDto>.Fail("Đơn đã tạo nhưng không tải lại được chi tiết.");

                Logger.LogInfoIf(Debug, "[ORCH][OT] Created: OTId={OTId}", otRequestId);
                return ServiceResult<OTRequestDto>.Ok(fullResult.Data, "Đã gửi đơn tăng ca thành công.");
            }
            catch (Exception ex)
            {
                return InternalError<OTRequestDto>(ex, "Lỗi hệ thống khi tạo đơn tăng ca.");
            }
        }

        public async Task<ServiceResult<OTRequestDto>> UpdateAsync(
            OTRequestUpsertDto model, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[ORCH][OT] Update start: Id={Id} User={User}", model.Id, user.UserName);

                var valResult = await _validator.ValidateCreateAsync(model, user, ct);
                if (!valResult.IsSuccess)
                    return ServiceResult<OTRequestDto>.Fail(valResult.Message ?? "Dữ liệu tăng ca không hợp lệ.");

                var currentResult = await _query.GetFullDetailsAsync(model.Id, ct);
                if (!currentResult.IsSuccess || currentResult.Data == null)
                    return ServiceResult<OTRequestDto>.Fail("Không tìm thấy đơn tăng ca.");

                var hasDecided = currentResult.Data.ApprovalSteps?.Any(s => s.Status != DecisionType.Pending) ?? false;
                if (hasDecided && !user.Permission.IsAdmin())
                    return ServiceResult<OTRequestDto>.Fail("Đơn đã có cấp duyệt xử lý, không thể sửa.");

                var existingCodes = currentResult.Data.Details.Select(d => d.EmployeeCode).ToHashSet();
                var modelCodes = model.Employees.Select(e => e.EmployeeCode).ToHashSet();

                var toRemove = existingCodes.Except(modelCodes).ToList();
                var toAdd = model.Employees.Where(e => !existingCodes.Contains(e.EmployeeCode)).ToList();
                var toUpdate = model.Employees.Where(e => existingCodes.Contains(e.EmployeeCode)).ToList();

                foreach (var code in toRemove)
                {
                    var removeResult = await _otService.RemoveEmployeeAsync(model.Id, code, user, ct);
                    if (!removeResult.IsSuccess)
                        return ServiceResult<OTRequestDto>.Fail($"Lỗi khi xóa {code}: {removeResult.Message}");
                }

                if (toAdd.Count > 0)
                {
                    var addResult = await _otService.AddEmployeesAsync(model.Id, toAdd, user, ct);
                    if (!addResult.IsSuccess)
                        return ServiceResult<OTRequestDto>.Fail(addResult.Message ?? "Không thể thêm nhân viên.");
                }

                if (toUpdate.Count > 0)
                {
                    var updateResult = await _otService.UpdateEmployeeOTInfoAsync(model.Id, toUpdate, user, ct);
                    if (!updateResult.IsSuccess)
                        return ServiceResult<OTRequestDto>.Fail(updateResult.Message ?? "Không thể cập nhật giờ OT.");
                }

                var fullResult = await _query.GetFullDetailsAsync(model.Id, ct);
                if (!fullResult.IsSuccess || fullResult.Data == null)
                    return ServiceResult<OTRequestDto>.Fail("Đã cập nhật nhưng không tải lại được chi tiết.");

                Logger.LogInfoIf(Debug, "[ORCH][OT] Updated: Id={Id} (+{Add}/-{Remove}/~{Change})",
                    model.Id, toAdd.Count, toRemove.Count, toUpdate.Count);

                return ServiceResult<OTRequestDto>.Ok(fullResult.Data);
            }
            catch (Exception ex)
            {
                return InternalError<OTRequestDto>(ex, "Lỗi hệ thống khi cập nhật đơn tăng ca.");
            }
        }

        public async Task<ServiceResult> CancelAsync(
            int otRequestId, string reason, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[ORCH][OT] Cancel start: Id={Id} User={User}", otRequestId, user.UserName);
                var result = await _otService.CancelAsync(otRequestId, reason, user, ct);

                if (result.IsSuccess)
                    Logger.LogInfoIf(Debug, "[ORCH][OT] Cancelled: Id={Id}", otRequestId);
                else
                    Logger.LogWarnIf(Debug, "[ORCH][OT] Cancel failed: Id={Id} Msg={Msg}", otRequestId, result.Message);

                return result;
            }
            catch (Exception ex)
            {
                return InternalError(ex, "Lỗi hệ thống khi hủy đơn tăng ca.");
            }
        }

        public async Task<ServiceResult<OTRequestDto>> GetDetailsAsync(
            int otRequestId, CancellationToken ct = default)
        {
            try
            {
                return await _query.GetFullDetailsAsync(otRequestId, ct);
            }
            catch (Exception ex)
            {
                return InternalError<OTRequestDto>(ex, "Lỗi hệ thống khi lấy chi tiết đơn tăng ca.");
            }
        }

        public async Task<ServiceResult<OTBalanceDto>> GetBalanceSummaryAsync(
            string employeeCode, int year, CancellationToken ct = default)
        {
            try
            {
                var data = await _query.GetSimpleBalanceAsync(employeeCode, year, ct);
                return ServiceResult<OTBalanceDto>.Ok(data);
            }
            catch (Exception ex)
            {
                return InternalError<OTBalanceDto>(ex, "Lỗi hệ thống khi lấy số dư/hạn mức tăng ca.");
            }
        }

        public async Task<ServiceResult<List<OTSummaryDto>>> GetRecentHistoryAsync(
            string employeeCode, int limit = 5, CancellationToken ct = default)
        {
            try
            {
                var data = await _query.GetRecentSummaryAsync(employeeCode, limit, ct);
                return ServiceResult<List<OTSummaryDto>>.Ok(data);
            }
            catch (Exception ex)
            {
                return InternalError<List<OTSummaryDto>>(ex, "Lỗi hệ thống khi lấy lịch sử tăng ca.");
            }
        }

        public async Task<ServiceResult<PaginationResult<OTSummaryDto>>> GetPagedHistoryAsync(
            string? deptCode, ApprovalStatus? status, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default)
        {
            try
            {
                var data = await _query.GetPagedAsync(deptCode, status, fromDate, toDate, page, pageSize, ct);
                return ServiceResult<PaginationResult<OTSummaryDto>>.Ok(data);
            }
            catch (Exception ex)
            {
                return InternalError<PaginationResult<OTSummaryDto>>(ex, "Lỗi hệ thống khi lấy danh sách đơn tăng ca.");
            }
        }

        public async Task<ServiceResult<OTReconciliationResultDto>> ReconcileActualHoursAsync(
            DateTime date, string? deptCode, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug,
                    "[ORCH][OT] Reconcile start: Date={Date} Dept={Dept}", date, deptCode ?? "ALL");

                var result = await _reconciliation.ReconcileActualHoursAsync(date, deptCode, ct);
                Logger.LogInfoIf(Debug,
                    "[ORCH][OT] Reconcile completed: Date={Date} Dept={Dept}", date, deptCode ?? "ALL");

                return ServiceResult<OTReconciliationResultDto>.Ok(result);
            }
            catch (Exception ex)
            {
                return InternalError<OTReconciliationResultDto>(ex, "Lỗi hệ thống khi đối chiếu chấm công OT.");
            }
        }
    }
}

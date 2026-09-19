

using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Entities;
using FVN_REGISTER.Infrastructure.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FVN_REGISTER.Infrastructure.Services.Leaves
{
    public class LeaveService
        : BaseRequestCommandService<LeaveRequestUpsertDto, F03LeaveDay, LeaveRequestSubject>,
          ILeaveService
    {
        private readonly ILeaveValidator _validator;
        private readonly IAuthorizationService _authorization;
        private readonly IApprovalSelectionService _approvalSelections;

        protected override RequestModule ModuleKind => RequestModule.Leave;

        public LeaveService(
            IUnitOfWork uow,
            IApprovalWorkflowOrchestrator<LeaveRequestSubject> workflow,
            ILeaveValidator validator,
            IAuthorizationService authorization,
            IApprovalSelectionService approvalSelections,
            ILogger<BaseRequestCommandService<LeaveRequestUpsertDto, F03LeaveDay, LeaveRequestSubject>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, workflow, logger, options)
        {
            _validator = validator;
            _authorization = authorization;
            _approvalSelections = approvalSelections;
        }

        public override async Task<ServiceResult<int>> CreateAsync(
            LeaveRequestUpsertDto model, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[{Component}] CreateAsync start: {User}", ComponentName, user.EmployeeCode);

                var valResult = await _validator.ValidateAsync(model, user, ct);
                if (!valResult.IsSuccess)
                {
                    Logger.LogWarnIf(Debug, "[{Component}] Validation failed: {Msg}", ComponentName, valResult.Message);
                    return ServiceResult<int>.Fail(valResult.Message ?? "Dữ liệu không hợp lệ.");
                }

                if (model.Details == null || model.Details.Count == 0)
                    return ServiceResult<int>.Fail("Phải chọn ít nhất 1 ngày nghỉ.");

                var leaveType = await Uow.Repository<F03LeaveType>().Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.LeaveTypeCode == model.LeaveTypeCode && x.IsActive == true, ct);

                if (leaveType == null)
                    return ServiceResult<int>.Fail($"Loại nghỉ [{model.LeaveTypeCode}] không tồn tại hoặc đã ngưng hoạt động.");

                bool isCounted = leaveType.IsCountedAsLeave == true;

                await using var tx = await Uow.BeginTransactionAsync(ct);

                var entity = new F03LeaveDay
                {
                    EmployeeCode = user.EmployeeCode ?? "",
                    DeptCode = user.DeptCode,
                    RequestStatus = ApprovalStatus.Draft,
                    WorkYear = model.StartDate.Year,
                    LeaveReason = model.Reason ?? "",
                    LeaveTypeCode = model.LeaveTypeCode,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreatedBy = user.UserId,
                    CreatedAt = DateTime.Now
                };

                await Uow.Repository<F03LeaveDay>().AddAsync(entity, ct);
                await Uow.SaveChangesAsync(ct);

                decimal totalDay = 0, totalLeaveDay = 0;
                var detailRepo = Uow.Repository<F03LeaveDayDetail>();

                foreach (var d in model.Details)
                {
                    var dayValue = d.IsHalfDay ? 0.5m : 1m;
                    await detailRepo.AddAsync(new F03LeaveDayDetail
                    {
                        LeaveDaysId = entity.Id,
                        LeaveDate = d.LeaveDate,
                        LeaveTypeCode = model.LeaveTypeCode,
                        LeaveTypeName = leaveType.LeaveTypeName,
                        IsHalfDay = d.IsHalfDay,
                        HalfDayOption = d.HalfDayOption,
                        DayValue = dayValue,
                        IsCountedAsLeave = isCounted,
                        CreatedAt = DateTime.Now,
                        CreatedBy = user.UserId
                    }, ct);
                    totalDay += dayValue;
                    if (isCounted) totalLeaveDay += dayValue;
                }

                await Uow.SaveChangesAsync(ct);
                entity.TotalDay = totalDay;
                entity.TotalLeaveDay = totalLeaveDay;
                await Uow.SaveChangesAsync(ct);

                if (model.ApprovalSelections == null || model.ApprovalSelections.Count == 0)
                    return ServiceResult<int>.Fail("Vui lòng chọn người phê duyệt cho từng cấp.");

                await _approvalSelections.ReplaceAsync(RequestModule.Leave, entity.Id, model.ApprovalSelections, user.UserId, ct);

                var context = ApprovalBuildContext.ForLeave(
                    requestId: entity.Id,
                    employeeCode: user.EmployeeCode ?? "",
                    deptCode: user.DeptCode ?? "",
                    positionCode: user.PositionCode ?? "",
                    year: entity.WorkYear,
                    leaveTypeCode: model.LeaveTypeCode);

                try
                {
                    await Workflow.InitApprovalAsync(entity.Id, context, ct);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "[{Component}] InitApproval failed, rolling back CreateId={Id}", ComponentName, entity.Id);
                    await tx.RollbackAsync(ct);
                    return ServiceResult<int>.Fail("Không thể khởi tạo luồng duyệt cho đơn.");
                }

                await tx.CommitAsync(ct);
                Logger.LogInfoIf(Debug, "[{Component}] Created LeaveId={Id} By={User}", ComponentName, entity.Id, user.EmployeeCode);
                return ServiceResult<int>.Ok(entity.Id, "Đã gửi đơn nghỉ thành công.");
            }
            catch (Exception ex)
            {
                return InternalError<int>(ex, "Lỗi hệ thống khi tạo đơn nghỉ.");
            }
        }

        public async Task<ServiceResult<int>> UpdateAsync(
            LeaveRequestUpsertDto model, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[{Component}] UpdateAsync start: Id={Id} User={User}", ComponentName, model.Id, user.EmployeeCode);

                var entity = await Uow.Repository<F03LeaveDay>().Query()
                    .FirstOrDefaultAsync(x => x.Id == model.Id, ct);
                if (entity == null)
                    return ServiceResult<int>.Fail("Không tìm thấy đơn nghỉ.");

                if (!await _authorization.CanAccessAsync(
                    user, SecurityFunctionCodes.LeaveEdit, entity.EmployeeCode, entity.DeptCode, ct))
                    return ServiceResult<int>.Fail("Không có quyền sửa đơn này theo phạm vi dữ liệu được cấp.");
                if (IsFinalized(entity))
                    return ServiceResult<int>.Fail("Đơn đã xử lý xong, không thể sửa.");
                if (await HasAnyApprovalDecisionAsync(entity.Id, ct))
                    return ServiceResult<int>.Fail("Đơn đã có bước duyệt được xử lý, không thể sửa nội dung. Vui lòng hủy đơn và tạo lại nếu cần thay đổi.");

                var valResult = await _validator.ValidateAsync(model, user, ct);
                if (!valResult.IsSuccess)
                    return ServiceResult<int>.Fail(valResult.Message ?? "Dữ liệu không hợp lệ.");
                if (model.Details == null || model.Details.Count == 0)
                    return ServiceResult<int>.Fail("Phải chọn ít nhất 1 ngày nghỉ.");

                var leaveType = await Uow.Repository<F03LeaveType>().Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.LeaveTypeCode == model.LeaveTypeCode && x.IsActive == true, ct);
                if (leaveType == null)
                    return ServiceResult<int>.Fail($"Loại nghỉ [{model.LeaveTypeCode}] không tồn tại hoặc đã ngưng hoạt động.");

                bool isCounted = leaveType.IsCountedAsLeave == true;
                await using var tx = await Uow.BeginTransactionAsync(ct);
                try
                {
                    var detailRepo = Uow.Repository<F03LeaveDayDetail>();
                    var oldDetails = await detailRepo.Query()
                        .Where(x => x.LeaveDaysId == entity.Id)
                        .ToListAsync(ct);
                    foreach (var old in oldDetails)
                        detailRepo.Remove(old);
                    await Uow.SaveChangesAsync(ct);

                    decimal totalDay = 0, totalLeaveDay = 0;
                    foreach (var d in model.Details)
                    {
                        var dayValue = d.IsHalfDay ? 0.5m : 1m;
                        await detailRepo.AddAsync(new F03LeaveDayDetail
                        {
                            LeaveDaysId = entity.Id,
                            LeaveDate = d.LeaveDate,
                            LeaveTypeCode = model.LeaveTypeCode,
                            LeaveTypeName = leaveType.LeaveTypeName,
                            IsHalfDay = d.IsHalfDay,
                            HalfDayOption = d.HalfDayOption,
                            DayValue = dayValue,
                            IsCountedAsLeave = isCounted,
                            CreatedAt = DateTime.Now,
                            CreatedBy = user.UserId
                        }, ct);
                        totalDay += dayValue;
                        if (isCounted) totalLeaveDay += dayValue;
                    }

                    entity.LeaveTypeCode = model.LeaveTypeCode;
                    entity.StartDate = model.StartDate;
                    entity.EndDate = model.EndDate;
                    entity.LeaveReason = model.Reason ?? "";
                    entity.WorkYear = model.StartDate.Year;
                    entity.TotalDay = totalDay;
                    entity.TotalLeaveDay = totalLeaveDay;
                    entity.ModifiedBy = user.UserId;
                    entity.ModifiedAt = DateTime.Now;

                    await Uow.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    Logger.LogInfoIf(Debug, "[{Component}] Updated LeaveId={Id} By={User}", ComponentName, entity.Id, user.EmployeeCode);
                    return ServiceResult<int>.Ok(entity.Id, "Đã cập nhật đơn nghỉ thành công.");
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            }
            catch (Exception ex)
            {
                return InternalError<int>(ex, "Lỗi hệ thống khi cập nhật đơn nghỉ.");
            }
        }

        private async Task<bool> HasAnyApprovalDecisionAsync(int requestId, CancellationToken ct)
        {
            return await Uow.Repository<F03ApprovalHistory>().Query()
                .AnyAsync(h => h.RequestId == requestId && h.RequestType == ModuleKind, ct);
        }

        public async Task<ServiceResult> CancelDetailAsync(
            int detailId, string reason, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                var detailRepo = Uow.Repository<F03LeaveDayDetail>();
                var detail = await detailRepo.GetByIdAsync(detailId, ct);
                if (detail == null)
                    return ServiceResult.Fail("Không tìm thấy dòng chi tiết.");

                var leaveRepo = Uow.Repository<F03LeaveDay>();
                var parent = await leaveRepo.GetByIdAsync(detail.LeaveDaysId, ct);
                if (parent == null)
                    return ServiceResult.Fail("Không tìm thấy đơn nghỉ.");

                if (!await _authorization.CanAccessAsync(
                    user, SecurityFunctionCodes.LeaveCancel, parent.EmployeeCode, parent.DeptCode, ct))
                    return ServiceResult.Fail("Không có quyền hủy dòng chi tiết này theo phạm vi dữ liệu được cấp.");
                if (IsFinalized(parent))
                    return ServiceResult.Fail("Đơn đã xử lý xong, không thể hủy chi tiết.");

                var removedValue = detail.DayValue;
                var wasCounted = detail.IsCountedAsLeave;
                detailRepo.Remove(detail);
                await Uow.SaveChangesAsync(ct);

                var remaining = await Uow.Repository<F03LeaveDayDetail>().Query()
                    .Where(x => x.LeaveDaysId == parent.Id)
                    .CountAsync(ct);

                if (remaining == 0)
                {
                    ApplyCancel(parent, reason, user);
                    await Uow.SaveChangesAsync(ct);
                    Logger.LogInfoIf(Debug, "[{Component}] Last detail removed -> parent LeaveId={Id} auto-cancelled", ComponentName, parent.Id);
                    return ServiceResult.Ok("Đã hủy dòng chi tiết cuối cùng, đơn nghỉ đã được hủy tự động.");
                }

                parent.TotalDay -= removedValue;
                if (wasCounted) parent.TotalLeaveDay -= removedValue;
                await Uow.SaveChangesAsync(ct);
                Logger.LogInfoIf(Debug, "[{Component}] CancelDetail DetailId={Id} By={User}", ComponentName, detailId, user.EmployeeCode);
                return ServiceResult.Ok("Đã hủy dòng chi tiết thành công.");
            }
            catch (Exception ex)
            {
                return InternalError(ex, "Lỗi hệ thống khi hủy dòng chi tiết.");
            }
        }

        protected override async Task<string?> ValidateApprovalScopeAsync(
            List<int> ids, UserIdentityDto user, CancellationToken ct)
        {
            if (user.UserId <= 0)
                return "Phiên đăng nhập không hợp lệ.";

            var distinctIds = ids.Distinct().ToList();
            var entities = await Uow.Repository<F03LeaveDay>().Query()
                .AsNoTracking()
                .Where(x => distinctIds.Contains(x.Id) && x.IsActive == true)
                .Select(x => new { x.Id, x.EmployeeCode, x.DeptCode })
                .ToListAsync(ct);

            if (entities.Count != distinctIds.Count)
                return "Một hoặc nhiều đơn nghỉ không tồn tại hoặc đã ngừng hoạt động.";

            foreach (var entity in entities)
            {
                if (!await _authorization.CanAccessAsync(
                    user,
                    SecurityFunctionCodes.LeaveApprove,
                    entity.EmployeeCode,
                    entity.DeptCode,
                    ct))
                {
                    return $"Không có quyền duyệt đơn nghỉ [{entity.Id}] theo phạm vi dữ liệu được cấp.";
                }
            }

            return null;
        }

        public override async Task<ServiceResult> CancelAsync(
            int requestId, string reason, UserIdentityDto user, CancellationToken ct = default)
        {
            var entity = await Uow.Repository<F03LeaveDay>().Query()
                .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct);

            if (entity == null)
                return ServiceResult.Fail("Không tìm thấy đơn nghỉ.");

            if (!await _authorization.CanAccessAsync(
                user, SecurityFunctionCodes.LeaveCancel, entity.EmployeeCode, entity.DeptCode, ct))
                return ServiceResult.Fail("Không có quyền hủy đơn này theo phạm vi dữ liệu được cấp.");

            if (IsFinalized(entity))
                return ServiceResult.Fail("Đơn đã xử lý xong, không thể hủy.");

            return await base.CancelAsync(requestId, reason, user, ct);
        }

        protected override void ApplyCancel(F03LeaveDay entity, string reason, UserIdentityDto user)
        {
            base.ApplyCancel(entity, reason, user);
            entity.LeaveReason = $"[Cancelled by {user.EmployeeCode}] {reason}";
            entity.ModifiedBy = user.UserId;
            entity.ModifiedAt = DateTime.Now;
        }
    }
}

using System.Text.Json;
using System.Linq.Expressions;
using FVN_REGISTER.Application.Interfaces.Execution;
using FVN_REGISTER.Application.Services.Execution;
using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Constants;
using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Application.Interfaces.HrmSync;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Execution;

public sealed class ExecutionHrResolutionService : IExecutionHrResolutionService
{
    private readonly IHrmAttendanceCalculationService _attendanceCalculation;
    private readonly IAuthorizationService _authorization;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ExecutionHrResolutionService> _logger;
    public const int HrExecutionReviewFunctionCode = FVN_REGISTER.Core.Constants.SecurityFunctionCodes.ExecutionReview;

    private readonly FVNWEBAPPContext _db;

    public ExecutionHrResolutionService(
        FVNWEBAPPContext db,
        IHrmAttendanceCalculationService attendanceCalculation,
        IAuthorizationService authorization,
        INotificationService notificationService,
        ILogger<ExecutionHrResolutionService> logger)
    {
        _db = db;
        _attendanceCalculation = attendanceCalculation;
        _authorization = authorization;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExecutionHrReviewItemDto>> GetPendingAsync(
        int userId,
        string? moduleCode,
        string? status,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        await EnsureHrPermissionAsync(userId, requireAllScope: false, cancellationToken);

        var actor = await _db.Users.AsNoTracking()
            .Where(x => x.Id == userId && x.IsActive != false)
            .Select(x => new { x.EmployeeCode, x.DeptCode })
            .SingleAsync(cancellationToken);

        var scope = await _authorization.GetScopeAsync(
            userId,
            HrExecutionReviewFunctionCode,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(scope)
            || string.Equals(scope, AuthorizationScopeCodes.None, StringComparison.OrdinalIgnoreCase))
            throw new FVN_REGISTER.Core.Exceptions.ForbiddenAccessException(
                "Execution Review chưa được cấp data scope.");

        var query = _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.IsActive != false
                && x.ReconciliationStatus != "Resolved"
                && (x.ReconciliationStatus == "Mismatch"
                    || x.ReconciliationStatus == "AwaitingConfirmation")
                && _db.ExecutionPolicies.Any(p =>
                    p.IsActive != false
                    && p.ModuleCode == x.ModuleCode
                    && p.ReviewMode != 0));

        if (!string.IsNullOrWhiteSpace(moduleCode))
            query = query.Where(x => x.ModuleCode == moduleCode.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.ReconciliationStatus == status.Trim());

        if (from.HasValue) query = query.Where(x => x.WorkDate >= from.Value);
        if (to.HasValue) query = query.Where(x => x.WorkDate <= to.Value);

        if (string.Equals(scope, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase))
            query = query.Where(x => _db.Employees.Any(e =>
                e.Id == x.EmployeeId && e.DeptCode == actor.DeptCode && e.IsActive != false));
        else if (string.Equals(scope, AuthorizationScopeCodes.Own, StringComparison.OrdinalIgnoreCase))
            query = query.Where(x => _db.Employees.Any(e =>
                e.Id == x.EmployeeId && e.EmployeeCode == actor.EmployeeCode && e.IsActive != false));

        var result = await query
            .Join(_db.Employees.AsNoTracking(),
                r => r.EmployeeId,
                e => e.Id,
                (r, e) => new ExecutionHrReviewItemDto(
                    r.Id, r.ModuleCode, r.SourceType, r.SourceId, r.ParticipantId,
                    r.EmployeeId, e.EmployeeCode, e.EmployeeName, r.WorkDate,
                    r.PlannedState, r.ActualState, r.ReconciliationStatus,
                    r.RequiresConfirmation, r.RequiresEvidence, r.ConfirmationId,
                    r.ActionId, r.ResolvedAt))
            .OrderBy(x => x.WorkDate)
            .ThenBy(x => x.ModuleCode)
            .ThenBy(x => x.EmployeeCode)
            .Take(500)
            .ToListAsync(cancellationToken);

        if (string.Equals(scope, AuthorizationScopeCodes.Employee, StringComparison.OrdinalIgnoreCase))
        {
            var filtered = new List<ExecutionHrReviewItemDto>(result.Count);
            foreach (var item in result)
            {
                if (await _authorization.CanAccessAsync(
                        new UserIdentityDto
                        {
                            UserId = userId,
                            EmployeeCode = actor.EmployeeCode,
                            DeptCode = actor.DeptCode,
                            IsLoggedIn = true
                        },
                        HrExecutionReviewFunctionCode,
                        item.EmployeeCode,
                        null,
                        cancellationToken))
                {
                    filtered.Add(item);
                }
            }

            return filtered;
        }

        return result;
    }

    public async Task<ExecutionReconciliationDetailDto?> GetDetailAsync(
        int userId,
        string employeeCode,
        long reconciliationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureHrPermissionAsync(userId, requireAllScope: false, cancellationToken);

        var target = await _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.Id == reconciliationId && x.IsActive != false)
            .Join(_db.Employees.AsNoTracking(),
                r => r.EmployeeId,
                e => e.Id,
                (r, e) => new { Reconciliation = r, Employee = e })
            .SingleOrDefaultAsync(cancellationToken);

        if (target is null)
            return null;

        if (string.Equals(target.Employee.EmployeeCode, employeeCode, StringComparison.OrdinalIgnoreCase))
            throw new FVN_REGISTER.Core.Exceptions.ForbiddenAccessException(
                "HR không được xem execution review của chính mình.");

        await EnsureHrTargetScopeAsync(
            userId,
            target.Employee.EmployeeCode,
            target.Employee.DeptCode,
            cancellationToken);

        var reconciliation = await _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.Id == reconciliationId)
            .Select(ToDto())
            .SingleAsync(cancellationToken);

        var confirmation = await _db.ExecutionConfirmations.AsNoTracking()
            .Where(x => x.ReconciliationId == reconciliationId && x.IsActive != false)
            .Select(x => new ExecutionConfirmationDto(
                x.Id, x.ReconciliationId, x.ModuleCode, x.SourceType, x.SourceId,
                x.ParticipantId, x.EmployeeId, x.WorkDate, x.Decision, x.Status,
                x.Comment, x.EvidenceRequired, x.SubmittedAt, x.ReviewedAt, x.ReviewNote))
            .FirstOrDefaultAsync(cancellationToken);

        var evidence = confirmation is null
            ? new List<ExecutionEvidenceDto>()
            : await _db.ExecutionConfirmationEvidence.AsNoTracking()
                .Where(x => x.ConfirmationId == confirmation.Id && x.IsActive != false)
                .OrderByDescending(x => x.SubmittedAt)
                .Select(x => new ExecutionEvidenceDto(
                    x.Id, x.ConfirmationId, x.EvidenceType, x.FileId,
                    x.ReferenceNo, x.ExternalUrl, x.Description,
                    x.ReviewStatus, x.SubmittedAt, x.ReviewedAt, x.ReviewNote))
                .ToListAsync(cancellationToken);

        var hrResolution = await _db.Set<F03ExecutionResolution>().AsNoTracking()
            .Where(x => x.ReconciliationId == reconciliationId && x.IsActive != false)
            .OrderByDescending(x => x.ResolvedAt)
            .Select(x => new ExecutionHrResolutionSummaryDto(
                x.Id, x.Decision, x.Reason, x.CalendarAction, x.ResolvedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return new ExecutionReconciliationDetailDto(
            reconciliation,
            confirmation,
            evidence,
            hrResolution);
    }

    private static Expression<Func<F03ExecutionReconciliation, ExecutionReconciliationDto>> ToDto() =>
        x => new ExecutionReconciliationDto(
            x.Id, x.ModuleCode, x.SourceType, x.SourceId, x.ParticipantId,
            x.EmployeeId, x.WorkDate, x.PlannedState, x.ActualState,
            x.ReconciliationStatus, x.RequiresConfirmation, x.RequiresEvidence,
            x.ConfirmationId, x.ActionId);

    public async Task<ExecutionEvidenceDto> ReviewEvidenceAsync(
        int userId,
        string employeeCode,
        long evidenceId,
        ExecutionEvidenceReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureHrPermissionAsync(userId, requireAllScope: false, cancellationToken);

        var reviewStatus = NormalizeEvidenceReviewStatus(request.ReviewStatus);
        var note = request.ReviewNote?.Trim();

        if (note?.Length > 2000)
            throw new ArgumentException("ReviewNote tối đa 2000 ký tự.");

        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);

        var evidence = await _db.ExecutionConfirmationEvidence
            .FirstOrDefaultAsync(x => x.Id == evidenceId && x.IsActive != false, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy evidence.");

        var confirmation = await _db.ExecutionConfirmations
            .FirstOrDefaultAsync(x => x.Id == evidence.ConfirmationId && x.IsActive != false, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy confirmation của evidence.");

        var reconciliation = await _db.ExecutionReconciliations
            .FirstOrDefaultAsync(x => x.Id == confirmation.ReconciliationId && x.IsActive != false, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy reconciliation của evidence.");

        if (string.Equals(reconciliation.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Reconciliation đã Resolved, không thể review evidence.");

        var policy = await _db.ExecutionPolicies.AsNoTracking()
            .Where(x => x.IsActive != false && x.ModuleCode == reconciliation.ModuleCode)
            .Select(x => new
            {
                x.ReviewMode,
                x.CorrectionMode
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (policy is null || policy.ReviewMode == 0)
            throw new InvalidOperationException(
                $"Module '{reconciliation.ModuleCode}' chưa bật HR Execution Review.");

        var targetEmployeeCode = await _db.Employees.AsNoTracking()
            .Where(x => x.Id == reconciliation.EmployeeId && x.IsActive != false)
            .Select(x => x.EmployeeCode)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhân viên của evidence.");

        if (string.Equals(employeeCode, targetEmployeeCode, StringComparison.OrdinalIgnoreCase))
            throw new FVN_REGISTER.Core.Exceptions.ForbiddenAccessException("HR không được review evidence của chính mình.");

        var targetDeptCode = await _db.Employees.AsNoTracking()
            .Where(x => x.EmployeeCode == targetEmployeeCode && x.IsActive != false)
            .Select(x => x.DeptCode)
            .SingleOrDefaultAsync(cancellationToken);

        await EnsureHrTargetScopeAsync(userId, targetEmployeeCode, targetDeptCode, cancellationToken);

        evidence.ReviewStatus = reviewStatus;
        evidence.ReviewedBy = userId;
        evidence.ReviewedAt = DateTime.Now;
        evidence.ReviewNote = note;
        evidence.ModifiedBy = userId;
        evidence.ModifiedAt = DateTime.Now;
        evidence.LastModifiedSource = "HR_EXECUTION_EVIDENCE_REVIEW";

        // Any negative evidence review keeps the employee action open.
        if (reviewStatus == "Rejected" || reviewStatus == "NeedMoreEvidence")
        {
            confirmation.Status = "NeedMoreEvidence";
            confirmation.ReviewedBy = userId;
            confirmation.ReviewedAt = DateTime.Now;
            confirmation.ReviewNote = note;
            reconciliation.RequiresConfirmation = true;
            reconciliation.RequiresEvidence = true;
            reconciliation.LastModifiedSource = "HR_EXECUTION_EVIDENCE_REVIEW";
        }

        var actorEmployeeId = await ResolveActorEmployeeIdAsync(employeeCode, userId, cancellationToken);

        _db.ExecutionReconciliationHistory.Add(new F03ExecutionReconciliationHistory
        {
            ReconciliationId = reconciliation.Id,
            FromStatus = reconciliation.ReconciliationStatus,
            ToStatus = reconciliation.ReconciliationStatus,
            EventType = $"EVIDENCE_{reviewStatus.ToUpperInvariant()}",
            Reason = note,
            ActorUserId = userId,
            ActorEmployeeId = actorEmployeeId,
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        try
        {
            await SendNotificationWithRetryAsync(
                () => NotifyEmployeeEvidenceReviewAsync(
                    reconciliation,
                    reviewStatus,
                    note,
                    cancellationToken),
                $"EvidenceReview:{reconciliation.Id}:{evidenceId}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Execution evidence review notification failed for ReconciliationId={ReconciliationId}, EvidenceId={EvidenceId}.",
                reconciliation.Id, evidenceId);
        }

        return new ExecutionEvidenceDto(
            evidence.Id,
            evidence.ConfirmationId,
            evidence.EvidenceType,
            evidence.FileId,
            evidence.ReferenceNo,
            evidence.ExternalUrl,
            evidence.Description,
            evidence.ReviewStatus,
            evidence.SubmittedAt,
            evidence.ReviewedAt,
            evidence.ReviewNote);
    }

    public async Task<ExecutionHrResolutionDto> ResolveAsync(
        int userId,
        string employeeCode,
        long reconciliationId,
        ExecutionHrResolutionRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureHrPermissionAsync(userId, requireAllScope: false, cancellationToken);

        var decision = NormalizeDecision(request.Decision);
        var calendarAction = NormalizeCalendarAction(request.CalendarAction);
        var reason = request.Reason?.Trim() ?? string.Empty;

        if (reason.Length == 0)
            throw new ArgumentException("Lý do xử lý là bắt buộc.");

        if (reason.Length > 2000)
            throw new ArgumentException("Lý do xử lý tối đa 2000 ký tự.");

        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);

        var reconciliation = await _db.ExecutionReconciliations
            .FirstOrDefaultAsync(x => x.Id == reconciliationId && x.IsActive != false, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy phản hồi đối soát.");

        if (reconciliation.ReconciliationStatus == "Resolved")
            throw new InvalidOperationException("Phản hồi này đã được HR giải quyết.");

        if (reconciliation.ReconciliationStatus != "Mismatch"
            && reconciliation.ReconciliationStatus != "AwaitingConfirmation")
            throw new InvalidOperationException(
                $"Trạng thái {reconciliation.ReconciliationStatus} không thuộc hàng chờ HR xử lý.");

        var now = DateTime.Now;
        var employee = await _db.Employees.AsNoTracking()
            .Where(x => x.Id == reconciliation.EmployeeId && x.IsActive != false)
            .Select(x => new { x.EmployeeCode, x.EmployeeName, x.DeptCode })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhân viên của phản hồi.");

        if (string.Equals(employee.EmployeeCode, employeeCode, StringComparison.OrdinalIgnoreCase))
            throw new FVN_REGISTER.Core.Exceptions.ForbiddenAccessException("HR không được tự giải quyết phản hồi của chính mình.");

        await EnsureHrTargetScopeAsync(userId, employee.EmployeeCode, employee.DeptCode, cancellationToken);

        var policy = await _db.ExecutionPolicies.AsNoTracking()
            .Where(x => x.IsActive != false && x.ModuleCode == reconciliation.ModuleCode)
            .Select(x => new
            {
                x.ReviewMode,
                x.CorrectionMode
            })
            .SingleOrDefaultAsync(cancellationToken);

        var reviewEnabled = policy is not null && policy.ReviewMode != 0;

        if (!reviewEnabled)
            throw new InvalidOperationException(
                $"Module '{reconciliation.ModuleCode}' chưa bật HR Execution Review.");

        var confirmation = reconciliation.ConfirmationId.HasValue
            ? await _db.ExecutionConfirmations.FirstOrDefaultAsync(
                x => x.Id == reconciliation.ConfirmationId.Value && x.IsActive != false,
                cancellationToken)
            : await _db.ExecutionConfirmations.FirstOrDefaultAsync(
                x => x.ReconciliationId == reconciliation.Id && x.IsActive != false,
                cancellationToken);

        if (decision == "OK" && reconciliation.RequiresEvidence && confirmation is null)
            throw new InvalidOperationException("Không thể Resolve OK khi chưa có confirmation.");

        if (confirmation is not null)
        {
            if (decision == "OK" && reconciliation.RequiresEvidence)
            {
                var hasApprovedEvidence = await _db.ExecutionConfirmationEvidence
                    .AsNoTracking()
                    .AnyAsync(x => x.ConfirmationId == confirmation.Id
                        && x.IsActive != false
                        && string.Equals(x.ReviewStatus, "Approved", StringComparison.OrdinalIgnoreCase),
                        cancellationToken);

                if (!hasApprovedEvidence)
                    throw new InvalidOperationException(
                        "Không thể Resolve OK khi chưa có evidence được HR review Approved.");
            }

            confirmation.Status = decision == "OK" ? "Approved" : "Rejected";
            // Confirmation.Decision belongs to the employee. HR's decision is
            // stored in F03ExecutionResolutions and must never overwrite it.
            confirmation.ReviewedBy = userId;
            confirmation.ReviewedAt = now;
            confirmation.ReviewNote = reason;
            confirmation.ModifiedBy = userId;
            confirmation.ModifiedAt = now;
            confirmation.LastModifiedSource = "HR_EXECUTION_REVIEW";
            reconciliation.ConfirmationId = confirmation.Id;
        }

        var calendar = await _db.CalendarProjections.FirstOrDefaultAsync(x =>
            x.IsActive != false
            && x.EmployeeId == reconciliation.EmployeeId
            && x.WorkDate == reconciliation.WorkDate
            && x.ModuleCode == reconciliation.ModuleCode
            && x.SourceType == reconciliation.SourceType
            && x.SourceId == reconciliation.SourceId
            && x.ParticipantId == reconciliation.ParticipantId,
            cancellationToken);

        var resolution = new F03ExecutionResolution
        {
            ReconciliationId = reconciliation.Id,
            Decision = decision,
            Reason = reason,
            CalendarAction = calendarAction,
            ResolvedByUserId = userId,
            ResolvedByEmployeeId = await ResolveActorEmployeeIdAsync(employeeCode, userId, cancellationToken),
            ResolvedAt = now,
            AppliedChangeJson = JsonSerializer.Serialize(new
            {
                Decision = decision,
                CalendarAction = calendarAction,
                CalendarProjectionId = calendar?.Id,
                ConfirmationId = confirmation?.Id
            })
        };
        _db.Set<F03ExecutionResolution>().Add(resolution);
        await _db.SaveChangesAsync(cancellationToken);

        if (decision == "OK"
            && policy.CorrectionMode != (byte)ExecutionCorrectionMode.None)
        {
            if (policy.CorrectionMode != (byte)ExecutionCorrectionMode.AttendanceRecalculate)
                throw new InvalidOperationException(
                    $"CorrectionMode={policy.CorrectionMode} chưa có correction handler.");

            var payrollPeriod = await _db.PayrollCalculationPeriods
                .AsNoTracking()
                .Where(x => x.IsActive != false
                    && x.FromDate <= reconciliation.WorkDate
                    && x.ToDate >= reconciliation.WorkDate)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Chưa có kỳ lương chứa ngày {reconciliation.WorkDate:dd/MM/yyyy}. Không thể thực hiện correction.");

            if (payrollPeriod.Status is "Locked" or "Exported")
                throw new InvalidOperationException(
                    $"Ngày {reconciliation.WorkDate:dd/MM/yyyy} thuộc kỳ lương {payrollPeriod.PeriodCode} đã {payrollPeriod.Status}. Không thể correction.");

            var correction = new F03ExecutionCorrection
            {
                ReconciliationId = reconciliation.Id,
                ResolutionId = resolution.Id,
                ModuleCode = reconciliation.ModuleCode,
                CorrectionType = policy.CorrectionMode.ToString(),
                EmployeeId = reconciliation.EmployeeId,
                WorkDate = reconciliation.WorkDate,
                Status = "Pending",
                RequestedState = decision,
                Reason = reason,
                CreatedBy = userId,
                CreatedAt = now,
                ModifiedBy = userId,
                ModifiedAt = now,
                LastModifiedSource = "HR_EXECUTION_CORRECTION"
            };
            _db.ExecutionCorrections.Add(correction);
            await _db.SaveChangesAsync(cancellationToken);

            var calc = await _attendanceCalculation.CalculateAsync(
                new FVN_REGISTER.Contract.Dtos.HrmSync.HrmAttendanceCalculationRequestDto
                {
                    DeptCode = employee.DeptCode,
                    FromDate = reconciliation.WorkDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = reconciliation.WorkDate.ToDateTime(TimeOnly.MinValue)
                },
                $"HR-EXECUTION-RESOLUTION:{reconciliationId}",
                cancellationToken);

            if (!calc.IsSuccess)
                throw new InvalidOperationException(
                    $"Không thể tính lại công ngày {reconciliation.WorkDate:dd/MM/yyyy}: {calc.Message}");

            correction.AppliedState = "RECALCULATED";
            correction.Status = "Applied";
            correction.AppliedAt = DateTime.Now;
            correction.AppliedBy = userId;
            correction.ModifiedBy = userId;
            correction.ModifiedAt = DateTime.Now;
            correction.LastModifiedSource = "HR_EXECUTION_CORRECTION";
            await _db.SaveChangesAsync(cancellationToken);
        }
        var oldStatus = reconciliation.ReconciliationStatus;
        reconciliation.ReconciliationStatus = "Resolved";
        reconciliation.ResolvedAt = now;
        reconciliation.ResolvedBy = userId;
        reconciliation.RequiresConfirmation = false;
        reconciliation.RequiresEvidence = false;
        reconciliation.LastModifiedSource = "HR_EXECUTION_REVIEW";

        if (reconciliation.ActionId.HasValue)
        {
            var action = await _db.ActionItems.FirstOrDefaultAsync(
                x => x.ActionId == reconciliation.ActionId.Value, cancellationToken);
            if (action is not null
                && action.Status != ActionItemStatus.Completed
                && action.Status != ActionItemStatus.Cancelled
                && action.Status != ActionItemStatus.Expired
                && action.Status != ActionItemStatus.Dismissed)
            {
                action.Status = ActionItemStatus.Completed;
                action.CompletedAt = now;
                action.ModifiedBy = userId;
                action.ModifiedAt = now;
                action.LastModifiedSource = "HR_EXECUTION_REVIEW";
            }
        }

        _db.ExecutionReconciliationHistory.Add(new F03ExecutionReconciliationHistory
        {
            ReconciliationId = reconciliation.Id,
            FromStatus = oldStatus,
            ToStatus = "Resolved",
            EventType = decision == "OK" ? "HR_RESOLUTION_OK" : "HR_RESOLUTION_NG",
            Reason = reason,
            ActorUserId = userId,
            ActorEmployeeId = resolution.ResolvedByEmployeeId,
            CreatedAt = now
        });

        await _db.SaveChangesAsync(cancellationToken);

        if (calendar is not null)
            ApplyCalendarResolution(calendar, decision, calendarAction, reason, DateTime.Now);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        try
        {
            await SendNotificationWithRetryAsync(
                () => CreateUserNotificationAsync(
                    reconciliation,
                    employee.EmployeeCode,
                    decision,
                    reason,
                    resolution.Id,
                    cancellationToken),
                $"HrResolution:{reconciliation.Id}:{resolution.Id}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Execution HR resolution notification failed for ReconciliationId={ReconciliationId}, ResolutionId={ResolutionId}.",
                reconciliation.Id, resolution.Id);
        }

        return new ExecutionHrResolutionDto(
            resolution.Id,
            reconciliation.Id,
            decision,
            reason,
            calendarAction,
            now);
    }

    private async Task EnsureHrPermissionAsync(
        int userId,
        bool requireAllScope,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users.AsNoTracking()
            .Where(x => x.Id == userId && x.IsActive != false)
            .Select(x => new
            {
                x.Id,
                x.EmployeeCode,
                x.DeptCode,
                x.PermissionCode,
                x.FullName,
                x.LevelApprove
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Tài khoản không tồn tại hoặc đã bị khóa.");

        var identity = new UserIdentityDto
        {
            UserId = user.Id,
            EmployeeCode = user.EmployeeCode,
            DeptCode = user.DeptCode,
            Permission = user.PermissionCode,
            FullName = user.FullName,
            LevelApprove = user.LevelApprove,
            IsLoggedIn = true
        };

        if (!await _authorization.HasAsync(identity, HrExecutionReviewFunctionCode, cancellationToken))
            throw new FVN_REGISTER.Core.Exceptions.ForbiddenAccessException("Tài khoản không có quyền Execution Review.");

        if (requireAllScope)
        {
            var scope = await _authorization.GetScopeAsync(userId, HrExecutionReviewFunctionCode, cancellationToken);
            if (!string.Equals(scope, FVN_REGISTER.Core.Constants.AuthorizationScopeCodes.All, StringComparison.OrdinalIgnoreCase))
                throw new FVN_REGISTER.Core.Exceptions.ForbiddenAccessException("Execution Review phải được cấp Scope=All.");
        }
    }

    private async Task NotifyEmployeeEvidenceReviewAsync(
        F03ExecutionReconciliation reconciliation,
        string reviewStatus,
        string? note,
        CancellationToken cancellationToken)
    {
        var targetEmployeeCode = await _db.Employees.AsNoTracking()
            .Where(x => x.Id == reconciliation.EmployeeId && x.IsActive != false)
            .Select(x => x.EmployeeCode)
            .SingleOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(targetEmployeeCode)) return;

        var user = await _db.Users.AsNoTracking()
            .Where(x => x.EmployeeCode == targetEmployeeCode && x.IsActive != false)
            .Select(x => new { x.Id, x.EmployeeCode })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) return;

        var module = ExecutionNotificationModuleMapper.ToRequestModule(reconciliation.ModuleCode);

        await _notificationService.CreateAsync(new FVN_REGISTER.Contract.Dtos.Notifications.CreateNotificationDto
        {
            UserId = user.Id,
            EmployeeCode = user.EmployeeCode,
            Module = module,
            Action = reviewStatus == "Approved" ? NotificationAction.Approved : NotificationAction.Rejected,
            Title = $"Evidence {reviewStatus}",
            Body = note ?? $"Evidence {reviewStatus}.",
            ActionUrl = $"/execution?reconciliationId={reconciliation.Id}",
            ActionId = reconciliation.ActionId,
            Metadata = JsonSerializer.Serialize(new { reconciliation.Id, reviewStatus, note }),
            NotificationType = "EXECUTION_EVIDENCE_REVIEW",
            IsHighPriority = reviewStatus != "Approved"
        }, cancellationToken);
    }

    private async Task EnsureHrTargetScopeAsync(
        int userId,
        string targetEmployeeCode,
        string? targetDeptCode,
        CancellationToken cancellationToken)
    {
        var actor = await _db.Users.AsNoTracking()
            .Where(x => x.Id == userId && x.IsActive != false)
            .Select(x => new UserIdentityDto
            {
                UserId = x.Id,
                EmployeeCode = x.EmployeeCode,
                DeptCode = x.DeptCode,
                Permission = x.PermissionCode,
                FullName = x.FullName,
                LevelApprove = x.LevelApprove,
                IsLoggedIn = true
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Tài khoản HR không tồn tại.");

        if (!await _authorization.CanAccessAsync(
                actor,
                HrExecutionReviewFunctionCode,
                targetEmployeeCode,
                targetDeptCode,
                cancellationToken))
        {
            throw new FVN_REGISTER.Core.Exceptions.ForbiddenAccessException(
                $"HR không có scope xử lý nhân viên {targetEmployeeCode}.");
        }
    }

    private async Task<int> ResolveActorEmployeeIdAsync(
        string employeeCode,
        int userId,
        CancellationToken cancellationToken)
    {
        return await _db.Employees.AsNoTracking()
            .Where(x => x.EmployeeCode == employeeCode && x.IsActive != false)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException(
                $"Không xác định được EmployeeId của HR UserId={userId}.");
    }

    private async Task CreateUserNotificationAsync(
        F03ExecutionReconciliation reconciliation,
        string employeeCode,
        string decision,
        string reason,
        long resolutionId,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users.AsNoTracking()
            .Where(x => x.EmployeeCode == employeeCode && x.IsActive != false)
            .Select(x => new { x.Id, x.EmployeeCode })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) return;

        var module = reconciliation.ModuleCode.ToUpperInvariant() switch
        {
            "OT" => RequestModule.Overtime,
            "LEAVE" => RequestModule.Leave,
            "TRIP" => RequestModule.Trip,
            "EQUIPMENT" => RequestModule.Equipment,
            _ => throw new InvalidOperationException(
                $"Không có mapping Notification Module cho Execution ModuleCode '{reconciliation.ModuleCode}'.")
        };

        await _notificationService.CreateAsync(new FVN_REGISTER.Contract.Dtos.Notifications.CreateNotificationDto
        {
            UserId = user.Id,
            EmployeeCode = user.EmployeeCode,
            Module = module,
            Action = decision == "OK" ? NotificationAction.Approved : NotificationAction.Rejected,
            Title = decision == "OK"
                ? "Phản hồi đã được Nhân sự xác nhận"
                : "Phản hồi chưa được Nhân sự chấp thuận",
            Body = $"Ngày {reconciliation.WorkDate:dd/MM/yyyy} - {reconciliation.ModuleCode}: {reason}",
            ActionUrl = $"/execution?reconciliationId={reconciliation.Id}",
            ActionId = reconciliation.ActionId,
            Metadata = JsonSerializer.Serialize(new
            {
                reconciliation.Id,
                reconciliation.ModuleCode,
                resolutionId,
                Decision = decision,
                Reason = reason
            }),
            NotificationType = "EXECUTION_HR_RESOLUTION",
            IsHighPriority = decision == "NG"
        }, cancellationToken);
    }

    private async Task SendNotificationWithRetryAsync(
        Func<Task> send,
        string operation,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await send();
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt == maxAttempts)
                    break;

                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Notification operation '{operation}' failed after {maxAttempts} attempts.",
            lastException);
    }

    private static void ApplyCalendarResolution(
        F03CalendarProjection calendar,
        string decision,
        string action,
        string reason,
        DateTime now)
    {
        calendar.RequiresAction = false;

        if (action == "CANCEL")
        {
            calendar.StatusCode = "Cancelled";
            calendar.Marker = null;
            calendar.Severity = 0;
        }
        else if (action == "REFRESH")
        {
            calendar.StatusCode = decision == "OK" ? "Confirmed" : calendar.StatusCode;
            calendar.Severity = decision == "OK" ? (byte)0 : calendar.Severity;
        }

        calendar.Summary = string.IsNullOrWhiteSpace(calendar.Summary)
            ? reason
            : $"{calendar.Summary} | HR: {reason}";
        calendar.CalculatedAt = now;
        calendar.ModifiedAt = now;
        calendar.LastModifiedSource = "HR_EXECUTION_REVIEW";
    }



    private static string NormalizeDecision(string value) => value?.Trim().ToUpperInvariant() switch
    {
        "OK" => "OK",
        "NG" => "NG",
        _ => throw new ArgumentException("Decision chỉ được là OK hoặc NG.")
    };

    private static string NormalizeEvidenceReviewStatus(string value) => value?.Trim().ToUpperInvariant() switch
    {
        "APPROVED" => "Approved",
        "REJECTED" => "Rejected",
        "NEEDMOREEVIDENCE" => "NeedMoreEvidence",
        "NEED_MORE_EVIDENCE" => "NeedMoreEvidence",
        _ => throw new ArgumentException("ReviewStatus chỉ được là Approved, Rejected hoặc NeedMoreEvidence.")
    };

    private static string NormalizeCalendarAction(string value) => value?.Trim().ToUpperInvariant() switch
    {
        "KEEP" => "KEEP",
        "REFRESH" => "REFRESH",
        "CANCEL" => "CANCEL",
        _ => throw new ArgumentException("CalendarAction chỉ được là KEEP, REFRESH hoặc CANCEL.")
    };
}
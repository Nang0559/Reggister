using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Execution;
using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Enums;
using Microsoft.EntityFrameworkCore;

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
        await EnsureHrPermissionAsync(userId, requireAllScope: true, cancellationToken);

        var query = _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.IsActive != false
                && x.ReconciliationStatus != "Resolved"
                && (x.ReconciliationStatus == "Mismatch"
                    || x.ReconciliationStatus == "AwaitingConfirmation"));

        if (!string.IsNullOrWhiteSpace(moduleCode))
            query = query.Where(x => x.ModuleCode == moduleCode.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.ReconciliationStatus == status.Trim());

        if (from.HasValue) query = query.Where(x => x.WorkDate >= from.Value);
        if (to.HasValue) query = query.Where(x => x.WorkDate <= to.Value);

        return await query
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
    }

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

        if (decision == "OK" && reconciliation.ModuleCode.Equals("ATTENDANCE", StringComparison.OrdinalIgnoreCase))
        {
            var payrollPeriod = await _db.PayrollCalculationPeriods
                .AsNoTracking()
                .Where(x => x.IsActive != false && x.FromDate <= reconciliation.WorkDate && x.ToDate >= reconciliation.WorkDate)
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
                CorrectionType = "ATTENDANCE_RECALCULATE",
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

            correction.Status = "Applied";
            correction.AppliedAt = DateTime.Now;
            correction.AppliedBy = userId;
            correction.AppliedState = "RECALCULATED";
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

        var module = reconciliation.ModuleCode.ToUpperInvariant() switch
        {
            "OT" => RequestModule.Overtime,
            "LEAVE" => RequestModule.Leave,
            "TRIP" => RequestModule.Trip,
            _ => RequestModule.Leave
        };

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
            "ATTENDANCE" => RequestModule.Attendance,
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
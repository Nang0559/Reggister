using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Execution;
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Execution;

public sealed class ExecutionHrResolutionService : IExecutionHrResolutionService
{
    private readonly IHrmAttendanceCalculationService _attendanceCalculation;
    public const int HrExecutionReviewFunctionCode = 2107;

    private readonly FVNWEBAPPContext _db;

    public ExecutionHrResolutionService(
        FVNWEBAPPContext db,
        IHrmAttendanceCalculationService attendanceCalculation)
    {
        _db = db;
        _attendanceCalculation = attendanceCalculation;
    }

    public async Task<IReadOnlyList<ExecutionHrReviewItemDto>> GetPendingAsync(
        string? moduleCode,
        string? status,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.IsActive != false
                && x.ReconciliationStatus != "Resolved"
                && (x.ReconciliationStatus == "Mismatch"
                    || x.ReconciliationStatus == "AwaitingConfirmation"));

        if (!string.IsNullOrWhiteSpace(moduleCode))
            query = query.Where(x => x.ModuleCode == moduleCode.Trim().ToUpper());

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
            .ToListAsync(cancellationToken);
    }

    public async Task<ExecutionEvidenceDto> ReviewEvidenceAsync(
        int userId,
        string employeeCode,
        long evidenceId,
        ExecutionEvidenceReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureHrPermissionAsync(userId, cancellationToken);

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

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

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
        await EnsureHrPermissionAsync(userId, cancellationToken);

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
            .Select(x => new { x.EmployeeCode, x.EmployeeName })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhân viên của phản hồi.");

        var confirmation = reconciliation.ConfirmationId.HasValue
            ? await _db.ExecutionConfirmations.FirstOrDefaultAsync(
                x => x.Id == reconciliation.ConfirmationId.Value && x.IsActive != false,
                cancellationToken)
            : await _db.ExecutionConfirmations.FirstOrDefaultAsync(
                x => x.ReconciliationId == reconciliation.Id && x.IsActive != false,
                cancellationToken);

        if (confirmation is not null)
        {
            if (decision == "OK" && reconciliation.RequiresEvidence)
            {
                var evidenceRows = await _db.ExecutionConfirmationEvidence
                    .AsNoTracking()
                    .Where(x => x.ConfirmationId == confirmation.Id && x.IsActive != false)
                    .Select(x => x.ReviewStatus)
                    .ToListAsync(cancellationToken);

                if (evidenceRows.Count == 0 || evidenceRows.Any(x => !string.Equals(x, "Approved", StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException(
                        "Không thể Resolve OK khi evidence chưa được review Approved đầy đủ.");
            }

            confirmation.Status = decision == "OK" ? "Approved" : "Rejected";
            confirmation.Decision = decision == "OK" ? "USER_CORRECT" : "USER_NOT_CORRECT";
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

        if (decision == "OK")
        {
            var payrollPeriod = await _db.PayrollCalculationPeriods
                .AsNoTracking()
                .Where(x => x.IsActive && x.FromDate <= reconciliation.WorkDate && x.ToDate >= reconciliation.WorkDate)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (payrollPeriod?.Status is "Locked" or "Exported")
                throw new InvalidOperationException(
                    $"Ngày {reconciliation.WorkDate:dd/MM/yyyy} thuộc kỳ lương {payrollPeriod.PeriodCode} đã {payrollPeriod.Status}. Phải xử lý qua Payroll Adjustment/Reopen.");

            var correction = new F03ExecutionCorrection
            {
                ReconciliationId = reconciliation.Id,
                ResolutionId = resolution.Id,
                ModuleCode = reconciliation.ModuleCode.ToUpperInvariant(),
                CorrectionType = reconciliation.ModuleCode.Equals("ATTENDANCE", StringComparison.OrdinalIgnoreCase)
                    ? "ATTENDANCE_RECALCULATE"
                    : "MODULE_RESOLUTION",
                EmployeeId = reconciliation.EmployeeId,
                WorkDate = reconciliation.WorkDate,
                Status = "Pending",
                RequestedState = reconciliation.PlannedState,
                Reason = reason,
                CreatedBy = userId,
                CreatedAt = now,
                LastModifiedSource = "HR_EXECUTION_REVIEW"
            };
            _db.ExecutionCorrections.Add(correction);
            await _db.SaveChangesAsync(cancellationToken);

            if (reconciliation.ModuleCode.Equals("ATTENDANCE", StringComparison.OrdinalIgnoreCase))
            {
                var calc = await _attendanceCalculation.CalculateAsync(
                    new FVN_REGISTER.Contract.Dtos.HrmSync.HrmAttendanceCalculationRequestDto
                    {
                        DeptCode = null,
                        FromDate = reconciliation.WorkDate.ToDateTime(TimeOnly.MinValue),
                        ToDate = reconciliation.WorkDate.ToDateTime(TimeOnly.MinValue)
                    },
                    $"HR-EXECUTION-RESOLUTION:{resolution.Id}",
                    cancellationToken);

                if (!calc.IsSuccess)
                {
                    correction.Status = "Failed";
                    correction.Reason = calc.Message ?? reason;
                    await _db.SaveChangesAsync(cancellationToken);
                    throw new InvalidOperationException(
                        $"Không thể tính lại công ngày {reconciliation.WorkDate:dd/MM/yyyy}: {calc.Message}");
                }

                correction.Status = "Applied";
                correction.AppliedAt = DateTime.Now;
                correction.AppliedBy = userId;
                correction.AppliedState = "RECALCULATED";
                await _db.SaveChangesAsync(cancellationToken);
            }
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
            if (action is not null && action.Status != ActionItemStatus.Completed)
            {
                action.Status = ActionItemStatus.Completed;
                action.CompletedAt = now;
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

        // Attendance is a calculated result, never a manual source-of-truth.
        // HR-OK creates a correction request by recalculating the affected
        // employee/day through the existing HRM-compatible procedure.
        if (decision == "OK" && reconciliation.ModuleCode.Equals("ATTENDANCE", StringComparison.OrdinalIgnoreCase))
        {
            var calc = await _attendanceCalculation.CalculateAsync(
                new FVN_REGISTER.Contract.Dtos.HrmSync.HrmAttendanceCalculationRequestDto
                {
                    DeptCode = null,
                    FromDate = reconciliation.WorkDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = reconciliation.WorkDate.ToDateTime(TimeOnly.MinValue)
                },
                $"HR-EXECUTION-RESOLUTION:{resolution.Id}",
                cancellationToken);

            if (!calc.IsSuccess)
            {
                throw new InvalidOperationException(
                    $"Không thể tính lại công ngày {reconciliation.WorkDate:dd/MM/yyyy}: {calc.Message}");
            }
        }

        if (calendar is not null)
            ApplyCalendarResolution(calendar, decision, calendarAction, reason, DateTime.Now);

        await CreateUserNotificationAsync(
            reconciliation,
            employee.EmployeeCode,
            decision,
            reason,
            resolution.Id,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ExecutionHrResolutionDto(
            resolution.Id,
            reconciliation.Id,
            decision,
            reason,
            calendarAction,
            now);
    }

    private async Task EnsureHrPermissionAsync(int userId, CancellationToken cancellationToken)
    {
        var allowed = await _db.UserFunctions.AsNoTracking()
            .Where(x => x.IdUser == userId && x.Function.FunctionCode == HrExecutionReviewFunctionCode)
            .AnyAsync(cancellationToken);

        if (!allowed)
        {
            allowed = await _db.UserRoles.AsNoTracking()
                .Where(x => x.IdUser == userId)
                .Join(_db.RoleFunctions.AsNoTracking(),
                    ur => ur.IdRole,
                    rf => rf.IdRole,
                    (ur, rf) => rf)
                .Join(_db.Functions.AsNoTracking(),
                    rf => rf.IdFunction,
                    f => f.Id,
                    (rf, f) => f.FunctionCode)
                .AnyAsync(x => x == HrExecutionReviewFunctionCode, cancellationToken);
        }

        if (!allowed)
            throw new UnauthorizedAccessException("Tài khoản không có quyền xử lý phản hồi thực tế.");
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

        var module = Enum.TryParse<RequestModule>(reconciliation.ModuleCode, true, out var parsed)
            ? parsed
            : RequestModule.Leave;

        _db.AppNotifications.Add(new F03AppNotification
        {
            UserId = user.Id,
            EmployeeCode = user.EmployeeCode,
            RequestModule = module,
            Action = decision == "OK" ? NotificationAction.Approved : NotificationAction.Rejected,
            Title = decision == "OK"
                ? "Phản hồi đã được Nhân sự xác nhận"
                : "Phản hồi chưa được Nhân sự chấp thuận",
            Body = $"Ngày {reconciliation.WorkDate:dd/MM/yyyy} - {reconciliation.ModuleCode}: {reason}",
            ActionUrl = $"/execution/reconciliations/{reconciliation.Id}",
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
            IsRead = false,
            IsHighPriority = decision == "NG"
        });
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
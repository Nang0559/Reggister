using System.Linq.Expressions;
using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Application.Interfaces.Execution;
using FVN_REGISTER.Application.Services.Execution;
using FVN_REGISTER.Application.Interfaces.Notifications;
using Microsoft.EntityFrameworkCore.Storage;
using FVN_REGISTER.Application.Models.Actions;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Infrastructure.Services.Execution;

public sealed class ExecutionReconciliationService : IExecutionReconciliationService
{
    private const string ConfirmationActionType = "EXECUTION_CONFIRMATION";

    private readonly FVNWEBAPPContext _db;
    private readonly IActionItemWriter _actionWriter;
    private readonly INotificationService _notificationService;
    private readonly IAuthorizationService _authorization;
    private readonly ILogger<ExecutionReconciliationService> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public ExecutionReconciliationService(
        FVNWEBAPPContext db,
        IActionItemWriter actionWriter,
        INotificationService notificationService,
        IAuthorizationService authorization,
        ILogger<ExecutionReconciliationService> logger,
        IHostEnvironment hostEnvironment)
    {
        _db = db;
        _actionWriter = actionWriter;
        _notificationService = notificationService;
        _authorization = authorization;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public async Task<ExecutionReconciliationDto?> GetAsync(string employeeCode, long reconciliationId, CancellationToken cancellationToken = default)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        return await _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.Id == reconciliationId && x.IsActive != false && x.EmployeeId == employeeId)
            .Select(ToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExecutionReconciliationDetailDto?> GetDetailAsync(
        string employeeCode,
        long reconciliationId,
        CancellationToken cancellationToken = default)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);

        var reconciliation = await _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.Id == reconciliationId
                && x.IsActive != false
                && x.EmployeeId == employeeId)
            .Select(ToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (reconciliation is null)
            return null;

        var confirmation = await _db.ExecutionConfirmations.AsNoTracking()
            .Where(x => x.ReconciliationId == reconciliationId
                && x.IsActive != false
                && x.EmployeeId == employeeId)
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

    public async Task<IReadOnlyList<ExecutionReconciliationDto>> GetMineAsync(string employeeCode, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (to < from) throw new ArgumentException("Khoảng ngày không hợp lệ.");
        if (to.DayNumber - from.DayNumber > 93) throw new ArgumentException("Khoảng ngày tối đa là 94 ngày.");

        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        return await _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.IsActive != false && x.EmployeeId == employeeId && x.WorkDate >= from && x.WorkDate <= to)
            .OrderBy(x => x.WorkDate).ThenBy(x => x.ModuleCode).ThenBy(x => x.SourceId).ThenBy(x => x.ParticipantId)
            .Select(ToDto()).ToListAsync(cancellationToken);
    }

    public async Task<ExecutionReconciliationDto> UpsertAsync(
        string employeeCode,
        ExecutionReconciliationUpsertRequest request,
        CancellationToken cancellationToken = default,
        int? actorUserId = null)
    {
        ValidateRequest(request);

        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        var effectiveActorUserId = actorUserId ?? await ResolveUserIdAsync(employeeCode, cancellationToken);
        if (actorUserId is null && request.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("Reconciliation không thuộc nhân viên hiện tại.");

        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);

        var entity = await _db.ExecutionReconciliations.FirstOrDefaultAsync(x =>
            x.ModuleCode == request.ModuleCode &&
            x.SourceType == request.SourceType &&
            x.SourceId == request.SourceId &&
            x.ParticipantId == request.ParticipantId &&
            x.EmployeeId == request.EmployeeId &&
            x.WorkDate == request.WorkDate, cancellationToken);

        var isNew = entity is null;
        var previousStatus = entity?.ReconciliationStatus;
        var previousActionId = entity?.ActionId;

        if (entity is null)
        {
            entity = new F03ExecutionReconciliation
            {
                ModuleCode = request.ModuleCode.Trim().ToUpperInvariant(),
                SourceType = request.SourceType.Trim().ToUpperInvariant(),
                SourceId = request.SourceId.Trim(),
                ParticipantId = request.ParticipantId?.Trim(),
                EmployeeId = request.EmployeeId,
                WorkDate = request.WorkDate,
                CreatedBy = effectiveActorUserId,
                CreatedAt = DateTime.Now
            };
            _db.ExecutionReconciliations.Add(entity);
        }

        // Resolved is terminal. AwaitingConfirmation is also sticky so a worker
        // rerun cannot reopen/regress an employee confirmation back to Mismatch.
        var requestedStatus = NormalizeStatus(request.ReconciliationStatus);
        var effectiveStatus = ResolveEffectiveStatus(
            previousStatus,
            requestedStatus,
            request.RequiresConfirmation);

        if (!string.Equals(previousStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
        {
            entity.PlannedState = request.PlannedState?.Trim();
            entity.ActualState = request.ActualState?.Trim();
            entity.ReconciliationStatus = effectiveStatus;
            entity.RequiresConfirmation = request.RequiresConfirmation;
            entity.RequiresEvidence = request.RequiresEvidence;
            entity.DetailJson = request.DetailJson;
            entity.ModifiedBy = effectiveActorUserId;
            entity.ModifiedAt = DateTime.Now;
            entity.LastModifiedSource = "EXECUTION_RECONCILIATION";

            if (string.Equals(requestedStatus, "Resolved", StringComparison.OrdinalIgnoreCase)
                && previousStatus is not null)
            {
                entity.ResolvedAt = DateTime.Now;
                entity.ResolvedBy = effectiveActorUserId;
                entity.LastModifiedSource = "EXECUTION_CANCELLED";
            }
        }

        var policy = await _db.ExecutionPolicies.AsNoTracking()
            .Where(x => x.IsActive != false && x.ModuleCode == entity.ModuleCode)
            .Select(x => new
            {
                x.ConfirmationMode,
                x.EvidenceMode,
                x.ReviewMode,
                x.AutoResolveMode,
                x.DueHours
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (policy is not null
            && policy.AutoResolveMode != 0
            && string.Equals(entity.ReconciliationStatus, "Matched", StringComparison.OrdinalIgnoreCase))
        {
            entity.ReconciliationStatus = "Resolved";
            entity.ResolvedAt = DateTime.Now;
            entity.ResolvedBy = effectiveActorUserId;
            entity.RequiresConfirmation = false;
            entity.RequiresEvidence = false;
        }

        var confirmationRequired = entity.RequiresConfirmation
            && policy is not null
            && policy.ConfirmationMode != 0;

        entity.RequiresConfirmation = confirmationRequired;
        entity.RequiresEvidence = confirmationRequired
            && (entity.RequiresEvidence || (policy?.EvidenceMode ?? 0) != 0);

        if (isNew)
            await _db.SaveChangesAsync(cancellationToken);

        if (confirmationRequired && !string.Equals(entity.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
        {
            // Compute the deadline only when this reconciliation has no action yet.
            // Re-runs must preserve the original DueAt.
            var initialDueAt = entity.ActionId.HasValue || policy?.DueHours is not > 0
                ? (DateTime?)null
                : DateTime.Now.AddHours(policy.DueHours.Value);

            entity.ActionId = await _actionWriter.EnsureOpenAsync(new ActionItemDraft(
                entity.ModuleCode,
                entity.SourceId,
                entity.EmployeeId,
                entity.EmployeeId,
                null,
                entity.WorkDate,
                ConfirmationActionType,
                "Xác nhận đối soát thực tế",
                $"Cần xác nhận {entity.ModuleCode} / {entity.SourceId}.",
                1,
                100,
                initialDueAt,
                "/execution",
                null,
                JsonSerializer.Serialize(new
                {
                    entity.ModuleCode,
                    entity.SourceType,
                    entity.SourceId,
                    entity.ParticipantId,
                    entity.WorkDate,
                    ReconciliationId = entity.Id
                }),
                entity.SourceType,
                entity.ParticipantId,
                effectiveActorUserId), cancellationToken);
        }
        else if (!confirmationRequired && entity.ActionId.HasValue)
        {
            await CancelOpenActionAsync(entity.ActionId.Value, cancellationToken);
        }

        await SaveProjectionAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var currentStatus = entity.ReconciliationStatus;
        if (isNew || !string.Equals(previousStatus, currentStatus, StringComparison.OrdinalIgnoreCase))
        {
            await AddHistoryAsync(
                entity,
                previousStatus,
                currentStatus,
                isNew ? "CREATED" : requestedStatus == "Resolved" && previousStatus is not null ? "CANCELLED_CLOSED" : "UPSERT",
                null,
                effectiveActorUserId,
                employeeId,
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        if ((isNew || previousActionId != entity.ActionId)
            && entity.ActionId.HasValue
            && !string.Equals(entity.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await NotifyActionCreatedAsync(entity, employeeCode, cancellationToken);
            }
            catch (Exception ex)
            {
                // Notification delivery is secondary; reconciliation/action commit remains authoritative.
                // The error is logged so operators can diagnose delivery failures.
                _logger.LogError(
                    ex,
                    "Execution action notification failed for ReconciliationId={ReconciliationId}, ActionId={ActionId}.",
                    entity.Id,
                    entity.ActionId);
            }
        }

        return await _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.Id == entity.Id)
            .Select(ToDto()).SingleAsync(cancellationToken);
    }

    public async Task<ExecutionConfirmationDto> SubmitConfirmationAsync(string employeeCode, long reconciliationId, ExecutionConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        var decision = NormalizeConfirmationDecision(request.Decision);
        var comment = request.Comment?.Trim();

        if (comment?.Length > 2000)
            throw new ArgumentException("Comment tối đa 2000 ký tự.");

        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        var effectiveActorUserId = await ResolveUserIdAsync(employeeCode, cancellationToken);

        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);

        var reconciliation = await _db.ExecutionReconciliations.FirstOrDefaultAsync(x =>
            x.Id == reconciliationId && x.IsActive != false && x.EmployeeId == employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy reconciliation.");

        if (string.Equals(reconciliation.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Reconciliation đã Resolved, không thể gửi lại confirmation.");

        if (!reconciliation.RequiresConfirmation)
            throw new InvalidOperationException("Reconciliation này không còn yêu cầu confirmation.");

        var previousStatus = reconciliation.ReconciliationStatus;

        var confirmation = await _db.ExecutionConfirmations.FirstOrDefaultAsync(
            x => x.ReconciliationId == reconciliation.Id, cancellationToken);

        if (confirmation is null)
        {
            confirmation = new F03ExecutionConfirmation
            {
                ReconciliationId = reconciliation.Id,
                ModuleCode = reconciliation.ModuleCode,
                SourceType = reconciliation.SourceType,
                SourceId = reconciliation.SourceId,
                ParticipantId = reconciliation.ParticipantId,
                EmployeeId = reconciliation.EmployeeId,
                WorkDate = reconciliation.WorkDate,
                CreatedBy = effectiveActorUserId,
                CreatedAt = DateTime.Now
            };
            _db.ExecutionConfirmations.Add(confirmation);
        }
        else
        {
            confirmation.ModifiedBy = effectiveActorUserId;
            confirmation.ModifiedAt = DateTime.Now;
        }

        confirmation.Decision = decision;
        confirmation.Comment = comment;
        confirmation.EvidenceRequired = request.EvidenceRequired || reconciliation.RequiresEvidence;
        confirmation.Status = "Pending";
        confirmation.SubmittedAt = DateTime.Now;
        confirmation.LastModifiedSource = "EMPLOYEE_CONFIRMATION";

        reconciliation.ReconciliationStatus = "AwaitingConfirmation";
        reconciliation.ModifiedBy = effectiveActorUserId;
        reconciliation.ModifiedAt = DateTime.Now;
        reconciliation.LastModifiedSource = "EMPLOYEE_CONFIRMATION";

        await _db.SaveChangesAsync(cancellationToken);
        reconciliation.ConfirmationId = confirmation.Id;
        await _db.SaveChangesAsync(cancellationToken);

        if (!string.Equals(previousStatus, reconciliation.ReconciliationStatus, StringComparison.OrdinalIgnoreCase))
        {
            await AddHistoryAsync(
                reconciliation,
                previousStatus,
                reconciliation.ReconciliationStatus,
                "CONFIRMATION_SUBMITTED",
                comment,
                effectiveActorUserId,
                employeeId,
                cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return ToConfirmationDto(confirmation);
    }

    public async Task<ExecutionEvidenceDto> AddEvidenceAsync(
        string employeeCode,
        int userId,
        long confirmationId,
        ExecutionEvidenceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.EvidenceType))
            throw new ArgumentException("EvidenceType không được để trống.");

        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        var effectiveActorUserId = await ResolveUserIdAsync(employeeCode, cancellationToken);
        var confirmation = await _db.ExecutionConfirmations.FirstOrDefaultAsync(x =>
            x.Id == confirmationId && x.IsActive != false && x.EmployeeId == employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy confirmation.");

        if (string.Equals(confirmation.Status, "Approved", StringComparison.OrdinalIgnoreCase)
            || string.Equals(confirmation.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Confirmation đã được review, không thể thêm evidence.");

        if (request.FileId.HasValue)
        {
            var ownsFile = await _db.Attachments.AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.FileId.Value &&
                    x.IsActive != false &&
                    x.CreatedBy == userId, cancellationToken);

            if (!ownsFile)
                throw new UnauthorizedAccessException("File đính kèm không thuộc người dùng hiện tại.");
        }

        if (!string.IsNullOrWhiteSpace(request.ExternalUrl))
        {
            if (!Uri.TryCreate(request.ExternalUrl.Trim(), UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException("ExternalUrl chỉ được phép dùng http hoặc https.");
        }

        var evidence = new F03ExecutionConfirmationEvidence
        {
            ConfirmationId = confirmation.Id,
            EvidenceType = request.EvidenceType.Trim(),
            FileId = request.FileId,
            ReferenceNo = request.ReferenceNo?.Trim(),
            ExternalUrl = request.ExternalUrl?.Trim(),
            Description = request.Description?.Trim(),
            SubmittedBy = effectiveActorUserId,
            SubmittedAt = DateTime.Now,
            ReviewStatus = "Pending",
            CreatedBy = effectiveActorUserId,
            CreatedAt = DateTime.Now,
            LastModifiedSource = "EXECUTION_EVIDENCE"
        };

        _db.ExecutionConfirmationEvidence.Add(evidence);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await NotifyHrEvidenceAddedAsync(confirmation.ReconciliationId, evidence.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            // Evidence persistence is authoritative; notification delivery is retriable/observable.
            _logger.LogError(
                ex,
                "Execution evidence HR notification failed for ReconciliationId={ReconciliationId}, EvidenceId={EvidenceId}.",
                confirmation.ReconciliationId,
                evidence.Id);
        }

        return ToEvidenceDto(evidence);
    }

    public async Task<int> UploadEvidenceFileAsync(
        string employeeCode,
        int userId,
        long confirmationId,
        string fileName,
        string? contentType,
        long length,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        const long maxBytes = 10 * 1024 * 1024;

        if (length <= 0 || length > maxBytes)
            throw new ArgumentException("File evidence phải lớn hơn 0 và không vượt quá 10 MB.");

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx"
        };

        if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
            throw new ArgumentException("Định dạng file evidence không được hỗ trợ.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Tên file evidence không hợp lệ.");

        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        var confirmation = await _db.ExecutionConfirmations
            .FirstOrDefaultAsync(x =>
                x.Id == confirmationId
                && x.IsActive != false
                && x.EmployeeId == employeeId,
                cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy confirmation.");

        if (string.Equals(confirmation.Status, "Approved", StringComparison.OrdinalIgnoreCase)
            || string.Equals(confirmation.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Confirmation đã được review, không thể upload evidence.");

        if (content is null)
            throw new ArgumentNullException(nameof(content));

        var root = Path.Combine(_hostEnvironment.ContentRootPath, "uploads", "execution-evidence");
        var folder = Path.Combine(root, DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"));
        Directory.CreateDirectory(folder);

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var storedPath = Path.Combine(folder, storedName);

        await using (var output = new FileStream(
            storedPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await content.CopyToAsync(output, cancellationToken);
        }

        try
        {
            var module = ExecutionNotificationModuleMapper.ToRequestModule(confirmation.ModuleCode);

            var attachment = new F03Attachment
            {
                Module = module,
                RequestId = checked((int)confirmationId),
                FileName = Path.GetFileName(fileName),
                FilePath = Path.GetRelativePath(_hostEnvironment.ContentRootPath, storedPath),
                FileExtension = extension,
                FileSize = length,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                ModifiedBy = userId,
                ModifiedAt = DateTime.Now,
                IsActive = true,
                LastModifiedSource = "EXECUTION_EVIDENCE_UPLOAD"
            };

            _db.Attachments.Add(attachment);
            await _db.SaveChangesAsync(cancellationToken);
            return attachment.Id;
        }
        catch
        {
            try { File.Delete(storedPath); } catch { /* preserve original database error */ }
            throw;
        }
    }

    private async Task NotifyActionCreatedAsync(
        F03ExecutionReconciliation reconciliation,
        string employeeCode,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users.AsNoTracking()
            .Where(x => x.EmployeeCode == employeeCode && x.IsActive != false)
            .Select(x => new { x.Id, x.EmployeeCode })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) return;

        var module = ExecutionNotificationModuleMapper.ToRequestModule(reconciliation.ModuleCode);

        await _notificationService.CreateAsync(new FVN_REGISTER.Contract.Dtos.Notifications.CreateNotificationDto
        {
            UserId = user.Id,
            EmployeeCode = user.EmployeeCode,
            Module = module,
            RelatedRequestId = int.TryParse(reconciliation.SourceId, out var requestId) ? requestId : 0,
            Action = NotificationAction.Pending,
            Title = $"Cần xác nhận đối soát {reconciliation.ModuleCode}",
            Body = $"Ngày {reconciliation.WorkDate:dd/MM/yyyy} — Cần xác nhận {reconciliation.ModuleCode} / {reconciliation.SourceId}.",
            ActionUrl = $"/execution?reconciliationId={reconciliation.Id}",
            ActionId = reconciliation.ActionId,
            NotificationType = "EXECUTION_ACTION",
            Metadata = JsonSerializer.Serialize(new
            {
                reconciliation.Id,
                reconciliation.ModuleCode,
                reconciliation.SourceType,
                reconciliation.SourceId,
                reconciliation.ParticipantId
            })
        }, cancellationToken);
    }




    private async Task NotifyHrEvidenceAddedAsync(
        long reconciliationId,
        long evidenceId,
        CancellationToken cancellationToken)
    {
        var reconciliation = await _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.Id == reconciliationId && x.IsActive != false)
            .Select(x => new
            {
                x.Id,
                x.ModuleCode,
                x.SourceId,
                x.EmployeeId,
                x.WorkDate,
                x.ActionId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (reconciliation is null)
            return;

        var employee = await _db.Employees.AsNoTracking()
            .Where(x => x.Id == reconciliation.EmployeeId && x.IsActive != false)
            .Select(x => new { x.EmployeeCode, x.DeptCode, x.EmployeeName })
            .SingleOrDefaultAsync(cancellationToken);

        if (employee is null)
            return;

        var reviewEnabled = await _db.ExecutionPolicies.AsNoTracking()
            .AnyAsync(x => x.IsActive != false
                && x.ModuleCode == reconciliation.ModuleCode
                && x.ReviewMode != 0, cancellationToken);

        if (!reviewEnabled)
            return;

        // Query candidates by the ExecutionReview function first. Scope is evaluated
        // from the same grant rows in-memory, so this path avoids N+1 HasAsync/CanAccessAsync calls.
        var roleCandidates = await (
            from ur in _db.UserRoles.AsNoTracking()
            join rf in _db.RoleFunctions.AsNoTracking() on ur.IdRole equals rf.IdRole
            join f in _db.Functions.AsNoTracking() on rf.IdFunction equals f.Id
            join u in _db.Users.AsNoTracking() on ur.IdUser equals u.Id
            where u.IsActive != false
                && u.EmployeeCode != employee.EmployeeCode
                && f.IsActive != false
                && f.FunctionCode == SecurityFunctionCodes.ExecutionReview
            select new
            {
                u.Id,
                u.EmployeeCode,
                u.DeptCode,
                u.PermissionCode,
                u.FullName,
                u.LevelApprove,
                f.ScopeCode
            })
            .ToListAsync(cancellationToken);

        var directCandidates = await (
            from uf in _db.UserFunctions.AsNoTracking()
            join f in _db.Functions.AsNoTracking() on uf.IdFunction equals f.Id
            join u in _db.Users.AsNoTracking() on uf.IdUser equals u.Id
            where u.IsActive != false
                && u.EmployeeCode != employee.EmployeeCode
                && f.IsActive != false
                && f.FunctionCode == SecurityFunctionCodes.ExecutionReview
            select new
            {
                u.Id,
                u.EmployeeCode,
                u.DeptCode,
                u.PermissionCode,
                u.FullName,
                u.LevelApprove,
                f.ScopeCode
            })
            .ToListAsync(cancellationToken);

        var candidates = roleCandidates
            .Concat(directCandidates)
            .GroupBy(x => new
            {
                x.Id,
                x.EmployeeCode,
                x.DeptCode,
                x.PermissionCode,
                x.FullName,
                x.LevelApprove
            })
            .Select(g => new
            {
                g.Key,
                Scopes = g.Select(x => x.ScopeCode)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            })
            .Where(x => x.Scopes.Any(scope =>
                string.Equals(scope, AuthorizationScopeCodes.All, StringComparison.OrdinalIgnoreCase)
                || (string.Equals(scope, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.Key.DeptCode, employee.DeptCode, StringComparison.OrdinalIgnoreCase))
                || ((string.Equals(scope, AuthorizationScopeCodes.Own, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(scope, AuthorizationScopeCodes.Employee, StringComparison.OrdinalIgnoreCase))
                    && string.Equals(x.Key.EmployeeCode, employee.EmployeeCode, StringComparison.OrdinalIgnoreCase))))
            .Select(x => new UserIdentityDto
            {
                UserId = x.Key.Id,
                EmployeeCode = x.Key.EmployeeCode,
                DeptCode = x.Key.DeptCode,
                Permission = x.Key.PermissionCode,
                FullName = x.Key.FullName,
                LevelApprove = x.Key.LevelApprove,
                IsLoggedIn = true
            })
            .ToList();

        var module = ExecutionNotificationModuleMapper.ToRequestModule(reconciliation.ModuleCode);
        var users = candidates.Select(x => x).ToList();

        foreach (var user in users)
        {
            if (!await _authorization.HasAsync(user, SecurityFunctionCodes.ExecutionReview, cancellationToken))
                continue;

            if (!await _authorization.CanAccessAsync(
                    user,
                    SecurityFunctionCodes.ExecutionReview,
                    employee.EmployeeCode,
                    employee.DeptCode,
                    cancellationToken))
                continue;

            await _notificationService.CreateAsync(
                new FVN_REGISTER.Contract.Dtos.Notifications.CreateNotificationDto
                {
                    UserId = user.UserId,
                    EmployeeCode = user.EmployeeCode,
                    Module = module,
                    Action = NotificationAction.Pending,
                    Title = $"Evidence mới cần review: {reconciliation.ModuleCode}",
                    Body = $"{employee.EmployeeCode} - {employee.EmployeeName}: evidence #{evidenceId} cho ngày {reconciliation.WorkDate:dd/MM/yyyy}.",
                    ActionUrl = $"/execution/hr?reconciliationId={reconciliation.Id}",
                    ActionId = reconciliation.ActionId,
                    NotificationType = "EXECUTION_EVIDENCE_REVIEW",
                    Metadata = JsonSerializer.Serialize(new
                    {
                        reconciliation.Id,
                        EvidenceId = evidenceId,
                        reconciliation.ModuleCode,
                        EmployeeCode = employee.EmployeeCode
                    })
                },
                cancellationToken);
        }
    }


    private async Task<int> ResolveEmployeeIdAsync(string employeeCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new UnauthorizedAccessException("Phiên đăng nhập không có mã nhân viên.");

        return await _db.Employees.AsNoTracking()
            .Where(x => x.IsActive != false && x.EmployeeCode == employeeCode)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhân viên của tài khoản hiện tại.");
    }

    private async Task<int> ResolveUserIdAsync(string employeeCode, CancellationToken cancellationToken)
    {
        return await _db.Users.AsNoTracking()
            .Where(x => x.IsActive != false && x.EmployeeCode == employeeCode)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Không xác định được UserId của tài khoản hiện tại.");
    }

    private static void ValidateRequest(ExecutionReconciliationUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ModuleCode)) throw new ArgumentException("ModuleCode không được để trống.");
        if (string.IsNullOrWhiteSpace(request.SourceType)) throw new ArgumentException("SourceType không được để trống.");
        if (string.IsNullOrWhiteSpace(request.SourceId)) throw new ArgumentException("SourceId không được để trống.");
        if (request.EmployeeId <= 0) throw new ArgumentException("EmployeeId không hợp lệ.");
        if (request.WorkDate == default) throw new ArgumentException("WorkDate không hợp lệ.");
        if (string.IsNullOrWhiteSpace(request.ReconciliationStatus)) throw new ArgumentException("ReconciliationStatus không được để trống.");
        _ = NormalizeStatus(request.ReconciliationStatus);
        if (request.DetailJson?.Length > 100000)
            throw new ArgumentException("DetailJson quá lớn.");
    }

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim();
        return normalized.ToLowerInvariant() switch
        {
            "none" => "None",
            "matched" => "Matched",
            "mismatch" => "Mismatch",
            "awaitingconfirmation" => "AwaitingConfirmation",
            "resolved" => "Resolved",
            _ => throw new ArgumentException($"ReconciliationStatus không hợp lệ: {status}.")
        };
    }

    private static string NormalizeConfirmationDecision(string value) => value?.Trim().ToUpperInvariant() switch
    {
        "CONFIRMED" => "CONFIRMED",
        "REJECTED" => "REJECTED",
        "NEED_MORE_EVIDENCE" => "NEED_MORE_EVIDENCE",
        _ => throw new ArgumentException("Decision chỉ được là CONFIRMED, REJECTED hoặc NEED_MORE_EVIDENCE.")
    };

    private static string ResolveEffectiveStatus(string? previousStatus, string requestedStatus, bool requiresConfirmation)
    {
        if (string.Equals(previousStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
            return "Resolved";

        if (string.Equals(previousStatus, "AwaitingConfirmation", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(requestedStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
            return "AwaitingConfirmation";

        if (requiresConfirmation && string.Equals(requestedStatus, "Mismatch", StringComparison.OrdinalIgnoreCase))
            return "Mismatch";

        return requestedStatus;
    }

    private async Task CancelOpenActionAsync(Guid actionId, CancellationToken cancellationToken)
    {
        var action = await _db.ActionItems.FirstOrDefaultAsync(
            x => x.ActionId == actionId
                && x.Status != ActionItemStatus.Completed
                && x.Status != ActionItemStatus.Dismissed
                && x.Status != ActionItemStatus.Cancelled
                && x.Status != ActionItemStatus.Expired,
            cancellationToken);

        if (action is null) return;

        action.Status = ActionItemStatus.Cancelled;
        action.CompletedAt = null;
        action.DismissedAt = null;
        action.ModifiedAt = DateTime.Now;
    }

    private async Task SaveProjectionAsync(
        F03ExecutionReconciliation reconciliation,
        CancellationToken cancellationToken)
    {
        var projection = await _db.CalendarProjections.FirstOrDefaultAsync(x =>
            x.IsActive != false
            && x.EmployeeId == reconciliation.EmployeeId
            && x.WorkDate == reconciliation.WorkDate
            && x.ModuleCode == reconciliation.ModuleCode
            && x.SourceType == reconciliation.SourceType
            && x.SourceId == reconciliation.SourceId
            && x.ParticipantId == reconciliation.ParticipantId,
            cancellationToken);

        if (string.Equals(reconciliation.LastModifiedSource, "EXECUTION_CANCELLED", StringComparison.OrdinalIgnoreCase))
        {
            if (projection is not null)
            {
                projection.IsActive = false;
                projection.RequiresAction = false;
                projection.Marker = null;
                projection.Severity = 0;
                projection.StatusCode = "Cancelled";
                projection.Summary = $"Đã hủy {reconciliation.ModuleCode}.";
                projection.ModifiedBy = reconciliation.ModifiedBy;
                projection.ModifiedAt = DateTime.Now;
                projection.LastModifiedSource = "EXECUTION_CANCELLED";
            }

            return;
        }

        if (projection is not null
            && string.Equals(reconciliation.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase)
            && string.Equals(projection.LastModifiedSource, "HR_EXECUTION_REVIEW", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (projection is null)
        {
            projection = new F03CalendarProjection
            {
                EmployeeId = reconciliation.EmployeeId,
                WorkDate = reconciliation.WorkDate,
                ModuleCode = reconciliation.ModuleCode,
                SourceType = reconciliation.SourceType,
                SourceId = reconciliation.SourceId,
                ParticipantId = reconciliation.ParticipantId,
                CreatedBy = reconciliation.ModifiedBy ?? reconciliation.CreatedBy,
                CreatedAt = DateTime.Now
            };
            _db.CalendarProjections.Add(projection);
        }

        projection.StatusCode = reconciliation.ReconciliationStatus;
        projection.Marker = reconciliation.ReconciliationStatus switch
        {
            "Mismatch" => "!",
            "AwaitingConfirmation" => "?",
            "Resolved" => "OK",
            _ => null
        };
        projection.Summary = reconciliation.ReconciliationStatus switch
        {
            "Mismatch" => $"Chênh lệch {reconciliation.ModuleCode}: cần xác nhận.",
            "AwaitingConfirmation" => $"Đang chờ xác nhận {reconciliation.ModuleCode}.",
            "Resolved" => $"Đã giải quyết {reconciliation.ModuleCode}.",
            _ => $"Đối soát {reconciliation.ModuleCode}: {reconciliation.ReconciliationStatus}."
        };
        projection.Severity = reconciliation.ReconciliationStatus == "Mismatch" ? (byte)2 :
            reconciliation.ReconciliationStatus == "AwaitingConfirmation" ? (byte)1 : (byte)0;
        projection.RequiresAction = reconciliation.RequiresConfirmation
            && !string.Equals(reconciliation.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase);
        projection.ActionId = reconciliation.ActionId;
        projection.DetailRoute = $"/execution?reconciliationId={reconciliation.Id}";
        projection.PayloadJson = reconciliation.DetailJson;
        projection.ModifiedBy = reconciliation.ModifiedBy;
        projection.ModifiedAt = DateTime.Now;
        projection.LastModifiedSource = "EXECUTION_RECONCILIATION";
        projection.CalculatedAt = DateTime.Now;
    }

    private Task AddHistoryAsync(
        F03ExecutionReconciliation reconciliation,
        string? fromStatus,
        string toStatus,
        string eventType,
        string? reason,
        int? effectiveActorUserId,
        int? actorEmployeeId,
        CancellationToken cancellationToken)
    {
        if (reconciliation.Id <= 0)
            return Task.CompletedTask;

        _db.ExecutionReconciliationHistory.Add(new F03ExecutionReconciliationHistory
        {
            ReconciliationId = reconciliation.Id,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            EventType = eventType,
            Reason = reason?.Length > 2000 ? reason[..2000] : reason,
            ActorUserId = effectiveActorUserId,
            ActorEmployeeId = actorEmployeeId,
            CreatedAt = DateTime.Now
        });

        return Task.CompletedTask;
    }

    private static Expression<Func<F03ExecutionReconciliation, ExecutionReconciliationDto>> ToDto() =>
        x => new ExecutionReconciliationDto(x.Id, x.ModuleCode, x.SourceType, x.SourceId, x.ParticipantId, x.EmployeeId, x.WorkDate,
            x.PlannedState, x.ActualState, x.ReconciliationStatus, x.RequiresConfirmation, x.RequiresEvidence, x.ConfirmationId, x.ActionId);

    private static ExecutionConfirmationDto ToConfirmationDto(F03ExecutionConfirmation x) =>
        new(x.Id, x.ReconciliationId, x.ModuleCode, x.SourceType, x.SourceId, x.ParticipantId, x.EmployeeId, x.WorkDate,
            x.Decision, x.Status, x.Comment, x.EvidenceRequired, x.SubmittedAt, x.ReviewedAt, x.ReviewNote);

    private static ExecutionEvidenceDto ToEvidenceDto(F03ExecutionConfirmationEvidence x) =>
        new(x.Id, x.ConfirmationId, x.EvidenceType, x.FileId, x.ReferenceNo, x.ExternalUrl, x.Description,
            x.ReviewStatus, x.SubmittedAt, x.ReviewedAt, x.ReviewNote);
}
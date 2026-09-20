using System.Linq.Expressions;
using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Application.Interfaces.Execution;
using Microsoft.EntityFrameworkCore.Storage;
using FVN_REGISTER.Application.Models.Actions;
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Execution;

public sealed class ExecutionReconciliationService : IExecutionReconciliationService
{
    private const string ConfirmationActionType = "EXECUTION_CONFIRMATION";

    private readonly FVNWEBAPPContext _db;
    private readonly IActionItemWriter _actionWriter;

    public ExecutionReconciliationService(FVNWEBAPPContext db, IActionItemWriter actionWriter)
    {
        _db = db;
        _actionWriter = actionWriter;
    }

    public async Task<ExecutionReconciliationDto?> GetAsync(string employeeCode, long reconciliationId, CancellationToken cancellationToken = default)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        return await _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.Id == reconciliationId && x.IsActive != false && x.EmployeeId == employeeId)
            .Select(ToDto())
            .FirstOrDefaultAsync(cancellationToken);
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

    public async Task<ExecutionReconciliationDto> UpsertAsync(string employeeCode, ExecutionReconciliationUpsertRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        if (request.EmployeeId != employeeId)
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

        if (entity is null)
        {
            entity = new F03ExecutionReconciliation
            {
                ModuleCode = request.ModuleCode,
                SourceType = request.SourceType,
                SourceId = request.SourceId,
                ParticipantId = request.ParticipantId,
                EmployeeId = request.EmployeeId,
                WorkDate = request.WorkDate
            };
            _db.ExecutionReconciliations.Add(entity);
        }

        // Resolved is terminal. A repeated projection/reconciliation run must not
        // regress a completed business lifecycle back to an open state.
        if (!string.Equals(entity.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
        {
            entity.PlannedState = request.PlannedState;
            entity.ActualState = request.ActualState;
            entity.ReconciliationStatus = NormalizeStatus(request.ReconciliationStatus);
            entity.RequiresConfirmation = request.RequiresConfirmation;
            entity.RequiresEvidence = request.RequiresEvidence;
            entity.DetailJson = request.DetailJson;
        }

        var previousStatus = entity.ReconciliationStatus;
        if (request.RequiresConfirmation && !string.Equals(entity.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
        {
            entity.ActionId = await _actionWriter.EnsureOpenAsync(new ActionItemDraft(
                request.ModuleCode,
                request.SourceId,
                request.EmployeeId,
                request.EmployeeId,
                null,
                request.WorkDate,
                ConfirmationActionType,
                "Xác nhận đối soát thực tế",
                $"Cần xác nhận {request.ModuleCode} / {request.SourceId}.",
                1,
                100,
                null,
                "/execution",
                null,
                JsonSerializer.Serialize(new
                {
                    request.ModuleCode,
                    request.SourceType,
                    request.SourceId,
                    request.ParticipantId,
                    request.WorkDate
                }),
                request.SourceType,
                request.ParticipantId), cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
        if (!string.Equals(previousStatus, entity.ReconciliationStatus, StringComparison.OrdinalIgnoreCase))
            await AddHistoryAsync(entity, previousStatus, entity.ReconciliationStatus, "UPSERT", null, null, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.Id == entity.Id)
            .Select(ToDto()).SingleAsync(cancellationToken);
    }

    public async Task<ExecutionConfirmationDto> SubmitConfirmationAsync(string employeeCode, long reconciliationId, ExecutionConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Decision))
            throw new ArgumentException("Decision không được để trống.");

        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);

        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);

        var reconciliation = await _db.ExecutionReconciliations.FirstOrDefaultAsync(x =>
            x.Id == reconciliationId && x.IsActive != false && x.EmployeeId == employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy reconciliation.");

        if (string.Equals(reconciliation.ReconciliationStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Reconciliation đã Resolved, không thể gửi lại confirmation.");

        if (!reconciliation.RequiresConfirmation)
            throw new InvalidOperationException("Reconciliation này không yêu cầu confirmation.");

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
                WorkDate = reconciliation.WorkDate
            };
            _db.ExecutionConfirmations.Add(confirmation);
        }

        confirmation.Decision = request.Decision.Trim();
        confirmation.Comment = request.Comment?.Trim();
        confirmation.EvidenceRequired = request.EvidenceRequired || reconciliation.RequiresEvidence;
        confirmation.Status = "Pending";
        confirmation.SubmittedAt = DateTime.Now;
        reconciliation.ReconciliationStatus = "AwaitingConfirmation";

        var previousStatus = reconciliation.ReconciliationStatus;
        await _db.SaveChangesAsync(cancellationToken);
        reconciliation.ConfirmationId = confirmation.Id;
        await _db.SaveChangesAsync(cancellationToken);
        if (!string.Equals(previousStatus, reconciliation.ReconciliationStatus, StringComparison.OrdinalIgnoreCase))
            await AddHistoryAsync(reconciliation, previousStatus, reconciliation.ReconciliationStatus, "CONFIRMATION_SUBMITTED", null, employeeId, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
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
            SubmittedBy = employeeId,
            SubmittedAt = DateTime.Now,
            ReviewStatus = "Pending"
        };

        _db.ExecutionConfirmationEvidence.Add(evidence);
        await _db.SaveChangesAsync(cancellationToken);
        return ToEvidenceDto(evidence);
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

    private static void ValidateRequest(ExecutionReconciliationUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ModuleCode)) throw new ArgumentException("ModuleCode không được để trống.");
        if (string.IsNullOrWhiteSpace(request.SourceType)) throw new ArgumentException("SourceType không được để trống.");
        if (string.IsNullOrWhiteSpace(request.SourceId)) throw new ArgumentException("SourceId không được để trống.");
        if (request.EmployeeId <= 0) throw new ArgumentException("EmployeeId không hợp lệ.");
        if (request.WorkDate == default) throw new ArgumentException("WorkDate không hợp lệ.");
        if (string.IsNullOrWhiteSpace(request.ReconciliationStatus)) throw new ArgumentException("ReconciliationStatus không được để trống.");
        _ = NormalizeStatus(request.ReconciliationStatus);
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

    private async Task AddHistoryAsync(
        F03ExecutionReconciliation reconciliation,
        string? fromStatus,
        string toStatus,
        string eventType,
        string? reason,
        int? actorEmployeeId,
        CancellationToken cancellationToken)
    {
        _db.ExecutionReconciliationHistory.Add(new F03ExecutionReconciliationHistory
        {
            ReconciliationId = reconciliation.Id,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            EventType = eventType,
            Reason = reason,
            ActorEmployeeId = actorEmployeeId,
            CreatedAt = DateTime.Now
        });
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
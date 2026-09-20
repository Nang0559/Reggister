using System.Linq.Expressions;
using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Application.Interfaces.Execution;
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

        entity.PlannedState = request.PlannedState;
        entity.ActualState = request.ActualState;
        entity.ReconciliationStatus = request.ReconciliationStatus;
        entity.RequiresConfirmation = request.RequiresConfirmation;
        entity.RequiresEvidence = request.RequiresEvidence;
        entity.DetailJson = request.DetailJson;

        if (request.RequiresConfirmation)
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

        return await _db.ExecutionReconciliations.AsNoTracking()
            .Where(x => x.Id == entity.Id)
            .Select(ToDto()).SingleAsync(cancellationToken);
    }

    public async Task<ExecutionConfirmationDto> SubmitConfirmationAsync(string employeeCode, long reconciliationId, ExecutionConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Decision)) throw new ArgumentException("Decision không được để trống.");

        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        var reconciliation = await _db.ExecutionReconciliations.FirstOrDefaultAsync(x =>
            x.Id == reconciliationId && x.IsActive != false && x.EmployeeId == employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy reconciliation.");

        var confirmation = await _db.ExecutionConfirmations.FirstOrDefaultAsync(x => x.ReconciliationId == reconciliation.Id, cancellationToken);
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
        confirmation.Comment = request.Comment;
        confirmation.EvidenceRequired = request.EvidenceRequired || reconciliation.RequiresEvidence;
        confirmation.Status = "Pending";
        confirmation.SubmittedAt = DateTime.Now;
        reconciliation.ReconciliationStatus = "AwaitingConfirmation";

        await _db.SaveChangesAsync(cancellationToken);
        reconciliation.ConfirmationId = confirmation.Id;
        await _db.SaveChangesAsync(cancellationToken);

        return ToConfirmationDto(confirmation);
    }

    public async Task<ExecutionEvidenceDto> AddEvidenceAsync(string employeeCode, long confirmationId, ExecutionEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.EvidenceType)) throw new ArgumentException("EvidenceType không được để trống.");

        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        var confirmation = await _db.ExecutionConfirmations.FirstOrDefaultAsync(x =>
            x.Id == confirmationId && x.IsActive != false && x.EmployeeId == employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy confirmation.");

        if (request.FileId.HasValue)
        {
            var exists = await _db.Attachments.AsNoTracking()
                .AnyAsync(x => x.Id == request.FileId.Value && x.IsActive != false, cancellationToken);
            if (!exists) throw new KeyNotFoundException("File đính kèm không tồn tại.");
        }

        var evidence = new F03ExecutionConfirmationEvidence
        {
            ConfirmationId = confirmation.Id,
            EvidenceType = request.EvidenceType.Trim(),
            FileId = request.FileId,
            ReferenceNo = request.ReferenceNo,
            ExternalUrl = request.ExternalUrl,
            Description = request.Description,
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
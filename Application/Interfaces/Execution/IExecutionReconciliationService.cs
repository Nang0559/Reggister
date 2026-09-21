using FVN_REGISTER.Contract.Dtos.Execution;

namespace FVN_REGISTER.Application.Interfaces.Execution;

public interface IExecutionReconciliationService
{
    Task<ExecutionReconciliationDto?> GetAsync(
        string employeeCode,
        long reconciliationId,
        CancellationToken cancellationToken = default);

    Task<ExecutionReconciliationDetailDto?> GetDetailAsync(
        string employeeCode,
        long reconciliationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExecutionReconciliationDto>> GetMineAsync(
        string employeeCode,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<ExecutionReconciliationDto> UpsertAsync(
        string employeeCode,
        ExecutionReconciliationUpsertRequest request,
        CancellationToken cancellationToken = default,
        int? actorUserId = null);

    Task<ExecutionConfirmationDto> SubmitConfirmationAsync(
        string employeeCode,
        long reconciliationId,
        ExecutionConfirmationRequest request,
        CancellationToken cancellationToken = default);

    Task<ExecutionEvidenceDto> AddEvidenceAsync(
        string employeeCode,
        int userId,
        long confirmationId,
        ExecutionEvidenceRequest request,
        CancellationToken cancellationToken = default);

    Task<int> UploadEvidenceFileAsync(
        string employeeCode,
        int userId,
        long confirmationId,
        string fileName,
        string? contentType,
        long length,
        Stream content,
        CancellationToken cancellationToken = default);
}
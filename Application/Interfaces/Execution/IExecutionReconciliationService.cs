using FVN_REGISTER.Contract.Dtos.Execution;

namespace FVN_REGISTER.Application.Interfaces.Execution;

public interface IExecutionReconciliationService
{
    Task<ExecutionReconciliationDto?> GetAsync(
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
        CancellationToken cancellationToken = default);

    Task<ExecutionConfirmationDto> SubmitConfirmationAsync(
        string employeeCode,
        long reconciliationId,
        ExecutionConfirmationRequest request,
        CancellationToken cancellationToken = default);

    Task<ExecutionEvidenceDto> AddEvidenceAsync(
        string employeeCode,
        long confirmationId,
        ExecutionEvidenceRequest request,
        CancellationToken cancellationToken = default);
}
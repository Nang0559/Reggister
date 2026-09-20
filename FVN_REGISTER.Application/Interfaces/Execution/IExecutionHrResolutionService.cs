using FVN_REGISTER.Contract.Dtos.Execution;

namespace FVN_REGISTER.Application.Interfaces.Execution;

public interface IExecutionHrResolutionService
{
    Task<IReadOnlyList<ExecutionHrReviewItemDto>> GetPendingAsync(
        string? moduleCode,
        string? status,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default);

    Task<ExecutionEvidenceDto> ReviewEvidenceAsync(
        int userId,
        string employeeCode,
        long evidenceId,
        ExecutionEvidenceReviewRequest request,
        CancellationToken cancellationToken = default);

    Task<ExecutionHrResolutionDto> ResolveAsync(
        int userId,
        string employeeCode,
        long reconciliationId,
        ExecutionHrResolutionRequest request,
        CancellationToken cancellationToken = default);
}
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Execution;

public interface IExecutionReconciliationClientService
{
    Task<ApiResponse<IReadOnlyList<ExecutionReconciliationDto>>> GetMineAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default);

    Task<ApiResponse<ExecutionReconciliationDto>> GetAsync(
        long reconciliationId,
        CancellationToken ct = default);

    Task<ApiResponse<ExecutionReconciliationDto>> UpsertAsync(
        ExecutionReconciliationUpsertRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<ExecutionConfirmationDto>> SubmitConfirmationAsync(
        long reconciliationId,
        ExecutionConfirmationRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<ExecutionEvidenceDto>> AddEvidenceAsync(
        long confirmationId,
        ExecutionEvidenceRequest request,
        CancellationToken ct = default);
}
using FVN_REGISTER.Contract.Dtos.Execution;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Execution;

public sealed class ExecutionReconciliationClientService : IExecutionReconciliationClientService
{
    private readonly IHttpClientWithAuth _http;

    public ExecutionReconciliationClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<IReadOnlyList<ExecutionReconciliationDto>>> GetMineAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default) =>
        _http.GetAsync<IReadOnlyList<ExecutionReconciliationDto>>(
            $"api/execution/me?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);

    public Task<ApiResponse<ExecutionReconciliationDetailDto>> GetDetailAsync(
        long reconciliationId, CancellationToken ct = default) =>
        _http.GetAsync<ExecutionReconciliationDetailDto>(
            $"api/execution/me/{reconciliationId}/detail", ct);

    public Task<ApiResponse<ExecutionReconciliationDto>> GetAsync(
        long reconciliationId, CancellationToken ct = default) =>
        _http.GetAsync<ExecutionReconciliationDto>(
            $"api/execution/me/{reconciliationId}", ct);

    public Task<ApiResponse<ExecutionConfirmationDto>> SubmitConfirmationAsync(
        long reconciliationId, ExecutionConfirmationRequest request, CancellationToken ct = default) =>
        _http.PostAsync<ExecutionConfirmationDto>(
            $"api/execution/me/{reconciliationId}/confirmation", request, ct);

    public Task<ApiResponse<ExecutionEvidenceDto>> AddEvidenceAsync(
        long confirmationId, ExecutionEvidenceRequest request, CancellationToken ct = default) =>
        _http.PostAsync<ExecutionEvidenceDto>(
            $"api/execution/me/confirmations/{confirmationId}/evidence", request, ct);
}
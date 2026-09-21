using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Contract.Requests.Users;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Users;

public sealed class PasswordResetRequestClientService : IPasswordResetRequestClientService
{
    private readonly IHttpClientWithAuth _http;

    public PasswordResetRequestClientService(IHttpClientWithAuth http) => _http = http;

    public Task<ApiResponse<object>> SubmitAsync(
        PasswordResetRequestCreateDto request,
        CancellationToken ct = default)
        => _http.PostAsync<object>("api/password-reset-requests", request, ct);

    public Task<ApiResponse<List<PasswordResetRequestDto>>> GetPendingAsync(
        CancellationToken ct = default)
        => _http.GetAsync<List<PasswordResetRequestDto>>(
            "api/password-reset-requests/pending", ct);

    public Task<ApiResponse<PasswordResetRequestDto>> ProcessAsync(
        int requestId,
        PasswordResetRequestProcessDto request,
        CancellationToken ct = default)
        => _http.PutAsync<PasswordResetRequestDto>(
            $"api/password-reset-requests/{requestId}/process", request, ct);
}

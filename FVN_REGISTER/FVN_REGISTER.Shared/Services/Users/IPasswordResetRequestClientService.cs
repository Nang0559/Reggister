using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Contract.Requests.Users;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Users;

public interface IPasswordResetRequestClientService
{
    Task<ApiResponse<object>> SubmitAsync(
        PasswordResetRequestCreateDto request,
        CancellationToken ct = default);

    Task<ApiResponse<List<PasswordResetRequestDto>>> GetPendingAsync(
        CancellationToken ct = default);

    Task<ApiResponse<PasswordResetRequestDto>> ProcessAsync(
        int requestId,
        PasswordResetRequestProcessDto request,
        CancellationToken ct = default);
}

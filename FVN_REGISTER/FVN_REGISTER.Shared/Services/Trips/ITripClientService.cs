using FVN_REGISTER.Contract.Dtos.Trips;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Trips;

public interface ITripClientService
{
    Task<ApiResponse<TripRequestDto>> CreateDraftAsync(
        CreateTripRequestDto request,
        CancellationToken ct = default);

    Task<ApiResponse<TripRequestDto>> SubmitAsync(
        int requestId,
        CancellationToken ct = default);

    Task<ApiResponse<TripRequestDto>> GetAsync(
        int requestId,
        CancellationToken ct = default);

    Task<ApiResponse<List<TripRequestDto>>> GetMineAsync(
        CancellationToken ct = default);
}

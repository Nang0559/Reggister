using FVN_REGISTER.Contract.Dtos.Trips;
using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Shared.Services.Trips;

public sealed class TripClientService : ITripClientService
{
    private readonly IHttpClientWithAuth _http;
    private readonly ILogger<TripClientService> _logger;

    public TripClientService(
        IHttpClientWithAuth http,
        ILogger<TripClientService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ApiResponse<TripRequestDto>> CreateDraftAsync(
        CreateTripRequestDto request,
        CancellationToken ct = default)
    {
        try
        {
            return await _http.PostAsync<TripRequestDto>("api/trips", request, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TRIP_CLIENT] CreateDraft error");
            return ApiResponse<TripRequestDto>.Fail("Không thể tạo đăng ký công tác.");
        }
    }

    public async Task<ApiResponse<TripRequestDto>> SubmitAsync(
        int requestId,
        List<ApprovalSelectionDto>? approvalSelections = null,
        CancellationToken ct = default)
    {
        try
        {
            return await _http.PostAsync<TripRequestDto>(
                $"api/trips/{requestId}/submit",
                approvalSelections ?? new List<ApprovalSelectionDto>(),
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TRIP_CLIENT] Submit error. RequestId={RequestId}", requestId);
            return ApiResponse<TripRequestDto>.Fail("Không thể gửi đăng ký công tác.");
        }
    }

    public async Task<ApiResponse<TripRequestDto>> GetAsync(
        int requestId,
        CancellationToken ct = default)
    {
        try
        {
            return await _http.GetAsync<TripRequestDto>($"api/trips/{requestId}", ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TRIP_CLIENT] Get error. RequestId={RequestId}", requestId);
            return ApiResponse<TripRequestDto>.Fail("Không thể tải đăng ký công tác.");
        }
    }

    public async Task<ApiResponse<List<TripRequestDto>>> GetMineAsync(
        CancellationToken ct = default)
    {
        try
        {
            return await _http.GetAsync<List<TripRequestDto>>("api/trips/mine", ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TRIP_CLIENT] GetMine error");
            return ApiResponse<List<TripRequestDto>>.Fail("Không thể tải lịch sử công tác.");
        }
    }
}

using FVN_REGISTER.Contract.Dtos.Trips;
using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.Trips;

public interface ITripService
{
    Task<TripRequestDto> CreateDraftAsync(CreateTripRequestDto request, CancellationToken ct = default);
    Task<TripRequestDto> UpdateDraftAsync(int requestId, CreateTripRequestDto request, CancellationToken ct = default);
    Task CancelAsync(int requestId, string reason, CancellationToken ct = default);
    Task<ServiceResult<TripRequestDto>> SubmitAsync(int requestId, List<ApprovalSelectionDto>? approvalSelections = null, CancellationToken ct = default);
    Task<TripRequestDto?> GetAsync(int requestId, CancellationToken ct = default);
    Task<List<TripRequestDto>> GetMineAsync(CancellationToken ct = default);
}

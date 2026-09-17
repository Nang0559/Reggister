
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;


namespace FVN_REGISTER.API.Services.Histories
{
    public class HistoryDispatcher : IHistoryDispatcher
    {
        private readonly IEnumerable<IHistoryHandler> _handlers;

        public HistoryDispatcher(IEnumerable<IHistoryHandler> handlers)
        {
            _handlers = handlers;
        }

        private IHistoryHandler Resolve(RequestModule kind)
            => _handlers.FirstOrDefault(h => h.Kind == kind)
               ?? throw new NotSupportedException($"No handler for kind: {kind}");

        public Task<ServiceResult<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
            HistoryFilterDto filter, UserIdentityDto user, CancellationToken ct)
            => Resolve(filter.Kind).GetHistoryAsync(filter, user, ct);

        public Task<ServiceResult<HistoryItemDetailDto>> GetDetailAsync(
            RequestModule kind, int id, UserIdentityDto user, CancellationToken ct)
            => Resolve(kind).GetDetailAsync(id, user, ct);

        public Task<ServiceResult<BalanceSummaryDto>> GetBalanceAsync(
            RequestModule kind, int year, UserIdentityDto user, CancellationToken ct)
            => Resolve(kind).GetBalanceAsync(year, user, ct);

        public Task<ServiceResult> CancelAsync(
            RequestModule kind, int id, string reason, UserIdentityDto user, CancellationToken ct)
            => Resolve(kind).CancelAsync(id, reason, user, ct);
    }
}

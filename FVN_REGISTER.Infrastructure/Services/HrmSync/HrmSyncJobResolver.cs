using FVN_REGISTER.Application.Interfaces.HrmSync;

namespace FVN_REGISTER.Infrastructure.Services.HrmSync
{
    public sealed class HrmSyncJobResolver : IHrmSyncJobResolver
    {
        private readonly IReadOnlyDictionary<string, IHrmSyncJob> _map;

        public HrmSyncJobResolver(IEnumerable<IHrmSyncJob> jobs)
        {
            _map = jobs.ToDictionary(x => x.EntityType, StringComparer.OrdinalIgnoreCase);
        }

        public IHrmSyncJob Resolve(string entityType)
        {
            if (_map.TryGetValue(entityType, out var job)) return job;
            throw new KeyNotFoundException($"Không có HRM sync job cho EntityType '{entityType}'.");
        }

        public IReadOnlyList<IHrmSyncJob> GetAll() => _map.Values.ToList();
    }
}

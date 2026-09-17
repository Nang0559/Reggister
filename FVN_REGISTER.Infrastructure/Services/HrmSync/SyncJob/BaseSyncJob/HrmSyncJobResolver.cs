using FVN_REGISTER.Application.Interfaces.HrmSync;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.BaseSyncJob
{
    public class HrmSyncJobResolver : IHrmSyncJobResolver
    {
        private readonly Dictionary<string, IHrmSyncJob> _jobs;

        public HrmSyncJobResolver(IEnumerable<IHrmSyncJob> jobs)
        {
            _jobs = jobs.ToDictionary(j => j.EntityType, StringComparer.OrdinalIgnoreCase);
        }

        public IHrmSyncJob Resolve(string entityType)
        {
            if (!_jobs.TryGetValue(entityType, out var job))
                throw new InvalidOperationException($"Không tìm thấy HrmSyncJob cho EntityType='{entityType}'.");
            return job;
        }

        public IReadOnlyList<IHrmSyncJob> GetAll()
             => _jobs.Values.OrderBy(j => j.SyncOrder).ToList();
    }
}

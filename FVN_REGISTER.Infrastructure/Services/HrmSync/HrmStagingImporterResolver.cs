using FVN_REGISTER.Application.Interfaces.HrmSync;

namespace FVN_REGISTER.Infrastructure.Services.HrmSync
{
    public sealed class HrmStagingImporterResolver : IHrmStagingImporterResolver
    {
        private readonly IReadOnlyDictionary<string, IHrmStagingImporter> _map;

        public HrmStagingImporterResolver(IEnumerable<IHrmStagingImporter> importers)
        {
            _map = importers.ToDictionary(x => x.EntityType, StringComparer.OrdinalIgnoreCase);
        }

        public IHrmStagingImporter Resolve(string entityType)
        {
            if (_map.TryGetValue(entityType, out var importer)) return importer;
            throw new KeyNotFoundException($"Không có HRM staging importer cho EntityType '{entityType}'.");
        }

        public IReadOnlyList<IHrmStagingImporter> GetAll() => _map.Values.ToList();
    }
}

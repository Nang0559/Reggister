namespace FVN_REGISTER.Application.Interfaces.HrmSync
{
    /// <summary>
    /// Resolve đúng importer theo EntityType — dùng bởi HrmImportWorker (chạy tất cả theo lịch)
    /// và admin UI (import theo yêu cầu, chỉ đúng 1 domain). Cùng pattern IHrmSyncJobResolver.
    /// </summary>
    public interface IHrmStagingImporterResolver
    {
        IHrmStagingImporter Resolve(string entityType);
        IReadOnlyList<IHrmStagingImporter> GetAll();
    }
}

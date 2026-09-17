namespace FVN_REGISTER.Application.Interfaces.HrmSync
{
    /// <summary>
    /// Non-generic marker — cho phép HrmImportWorker/Resolver giữ 1 collection
    /// gồm nhiều importer khác kiểu TStagingEntity, cùng pattern IHrmSyncJob.
    /// </summary>
    public interface IHrmStagingImporter
    {
        /// <summary>Khớp với F03StagingXxx.EntityType — dùng làm key cho Resolver.</summary>
        string EntityType { get; }

        /// <summary>
        /// Chủ động kéo dữ liệu từ HRM tại thời điểm asOfDate, ghi thô vào staging table.
        /// KHÔNG đánh dấu IsProcessed — đó là việc của HrmSyncJob ở bước sau.
        /// Trả về số dòng đã ghi vào staging.
        /// </summary>
        Task<int> ImportAsync(DateTime asOfDate, CancellationToken ct = default);
    }

    public interface IHrmStagingImporter<TStagingEntity> : IHrmStagingImporter
        where TStagingEntity : class
    {
    }
}

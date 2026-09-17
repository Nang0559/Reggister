namespace FVN_REGISTER.Application.Interfaces.HrmSync
{
    /// <summary>
    /// Resolve đúng job theo EntityType — dùng chung bởi HrmSyncWorker (poll tất cả)
    /// và các Management Service (sync theo yêu cầu, chỉ đúng domain của mình).
    /// Cùng pattern với IApprovalEngineResolver.
    /// </summary>
    public interface IHrmSyncJobResolver
    {
        IHrmSyncJob Resolve(string entityType);

        /// <summary>Dùng bởi HrmSyncWorker để lặp poll tất cả job đã đăng ký.</summary>
        IReadOnlyList<IHrmSyncJob> GetAll();
    }
}

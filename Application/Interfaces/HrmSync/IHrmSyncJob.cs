using FVN_REGISTER.Contract.Responses;


namespace FVN_REGISTER.Application.Interfaces.HrmSync
{
    public interface IHrmSyncJob
    {
        string EntityType { get; }
        /// <summary>Thứ tự chạy khi GetAll() — số nhỏ chạy trước. Mặc định 0 (song song về logic, không phụ thuộc FK).</summary>
        int SyncOrder => 0;
        bool IsBlockingDependency => false;
        Task<HrmSyncResult> RunAsync(CancellationToken ct = default);
    }
}

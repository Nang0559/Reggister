namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.BaseManuals
{
    using FVN_REGISTER.Application.Interfaces.HrmSync;
    using FVN_REGISTER.Core.Enums;
    using FVN_REGISTER.Core.Interfaces;
    using FVN_REGISTER.Core.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions;
    using System.Net.NetworkInformation;

    /// <summary>
    /// Base cho Importer — chứa logic diff dùng chung: pull HRM → so key với bảng đích →
    /// ghi staging kèm Action. Domain Importer chỉ khai báo cách map TSourceRow → TStaging
    /// và cách lấy EntityKey, không tự viết lại vòng lặp diff.
    /// </summary>
    public abstract class HrmStagingImporterBase<TSourceRow, TStaging, TEntity>
        : IHrmStagingImporter<TStaging>
        where TSourceRow : class
        where TStaging : class, IHrmStagingEntity
        where TEntity : class
    {
        private readonly IHrmSourceReader<TSourceRow> _reader;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _logger;

        public abstract string EntityType { get; }

        /// <summary>Giá trị ghi vào CreatedBy của staging — mặc định là tên importer, override nếu cần khác.</summary>
        protected virtual string CreatedBySystemTag => $"Importer:{EntityType}";

        protected HrmStagingImporterBase(
            IHrmSourceReader<TSourceRow> reader,
            IUnitOfWork uow,
            ILogger logger)
        {
            _reader = reader;
            _uow = uow;
            _logger = logger;
        }

        /// <summary>
        /// Chủ động kéo dữ liệu từ HRM tại thời điểm asOfDate, ghi thô vào staging.
        /// KHÔNG set IsProcessed=true — đó là việc của HrmSyncJob ở bước sau.
        /// Trả về tổng số dòng đã ghi vào staging (Upsert + Delete).
        /// </summary>
        public async Task<int> ImportAsync(DateTime asOfDate, CancellationToken ct = default)
        {
            int written = 0;

            try
            {
                // 1. Pull toàn bộ snapshot hiện tại từ HRM
                var sourceRows = await _reader.ReadAllAsync(ct);
                var sourceKeys = sourceRows.Select(GetSourceKey).ToHashSet();

                // 2. Lấy tập key hiện có ở bảng ĐÍCH — dùng lại chính entity đích,
                //    không cần bảng shadow riêng lưu "trạng thái nguồn lần trước".
                //    KHÔNG filter IsActive — khớp hành vi LoadExistingEntitiesAsync của
                //    HrmSyncJob (không filter), để record từng bị Delete có thể "hồi phục"
                //    đúng qua nhánh Update nếu xuất hiện lại ở nguồn.
                var existingKeys = await _uow.Repository<TEntity>()
                    .Query()
                    .Select(GetEntityKeySelector())
                    .ToListAsync(ct);

                var existingKeySet = existingKeys.ToHashSet();

                var stagingRepo = _uow.Repository<TStaging>();
                // MỚI — dọn các dòng CHƯA XỬ LÝ của cùng EntityType trước khi ghi lượt mới,
                // tránh tích lũy staging trùng key nếu HrmSyncJob chậm/bị gián đoạn nhiều chu kỳ Import.
                var stalePending = await stagingRepo.Query()
                    .Where(x => !x.IsProcessed)
                    .ToListAsync(ct);
                foreach (var stale in stalePending)
                    stagingRepo.Remove(stale);
                // 3. Mọi row còn tồn tại ở nguồn -> ghi staging Action=Update.
                //    KHÔNG tự phân biệt Insert/Update chính xác — HrmSyncJob
                //    (đã có LoadExistingEntitiesAsync) tự quyết định khi xử lý,
                //    tránh trùng lặp logic diff ở 2 tầng.
                foreach (var row in sourceRows)
                {
                    ct.ThrowIfCancellationRequested();

                    var staging = MapToStaging(row);
                    staging.Action = HrmChangeAction.Update;
                    staging.IsProcessed = false;
                    staging.ErrorMessage = null;
                    staging.CreatedAt = asOfDate;
                    staging.CreatedBy = CreatedBySystemTag;

                    await stagingRepo.AddAsync(staging, ct);
                    written++;
                }

                // 4. Key có ở entity đích nhưng KHÔNG còn trong source pull -> Action=Delete.
                //    Thông tin CHỈ Importer có tại thời điểm này.
                var missingKeys = existingKeySet.Except(sourceKeys).ToList();
                // Phòng vệ: nếu > 50 % dữ liệu đích "biến mất" trong 1 lần pull, nhiều khả năng
                // là lỗi đọc nguồn (timeout/partial result) chứ không phải HRM thật sự xóa hàng loạt.
                // Không tự Delete — chỉ log cảnh báo để người vận hành kiểm tra thủ công.
                if (existingKeySet.Count > 0 && missingKeys.Count > existingKeySet.Count / 2)
                {
                    _logger.LogWarning(
                        "[HRM-IMPORT] {EntityType} bất thường: {Missing}/{Total} key biến mất khỏi nguồn — " +
                        "bỏ qua Delete lượt này, nghi ngờ lỗi đọc nguồn HRM.",
                        EntityType, missingKeys.Count, existingKeySet.Count);
                }
                else
                {
                    foreach (var key in missingKeys)
                    {
                        ct.ThrowIfCancellationRequested();

                        var staging = BuildDeleteStaging(key);
                        staging.Action = HrmChangeAction.Delete;
                        staging.IsProcessed = false;
                        staging.ErrorMessage = null;
                        staging.CreatedAt = asOfDate;
                        staging.CreatedBy = CreatedBySystemTag;

                        await stagingRepo.AddAsync(staging, ct);
                        written++;
                    }
                }
                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "[HRM-IMPORT] {EntityType} DONE: Source={Source}, Written={Written}",
                    EntityType, sourceRows.Count, written);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HRM-IMPORT] {EntityType} ERROR", EntityType);
                throw; // để Worker/Resolver caller quyết định retry/log tiếp — không nuốt lỗi ở đây
            }

            return written;
        }

        // ================= HOOKS (domain override) =================

        protected abstract string GetSourceKey(TSourceRow row);

        protected abstract Expression<Func<TEntity, string>> GetEntityKeySelector();

        /// <summary>
        /// Map 1 dòng nguồn HRM sang bản ghi staging. KHÔNG cần set Id, Action, IsProcessed,
        /// ErrorMessage, CreatedAt, CreatedBy — base tự set các field chung này sau khi gọi hook.
        /// </summary>
        protected abstract TStaging MapToStaging(TSourceRow row);

        /// <summary>Dựng bản ghi staging tối thiểu cho trường hợp Delete (chỉ cần EntityKey).</summary>
        protected abstract TStaging BuildDeleteStaging(string entityKey);
    }
}
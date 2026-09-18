using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Interfaces;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.BaseSyncJob
{
    /// <summary>Gom các entity vừa xử lý trong 1 lần RunAsync — dùng cho AfterBatchAsync hook.</summary>
    public class HrmSyncBatchContext<TEntity>
    {
        public List<TEntity> Added { get; } = new();
        public List<TEntity> Updated { get; } = new();
        public List<TEntity> Deleted { get; } = new();
    }
    /// <summary>
    /// Khung xử lý đồng bộ 1 bảng HRM: đọc Staging chưa xử lý → dedupe theo EntityKey (lấy dòng mới
    /// nhất, các dòng cũ hơn bị "supersede") → Insert/Update/Delete vào bảng đích qua IUnitOfWork.
    /// Xử lý TỪNG DÒNG độc lập (không rollback cả batch nếu 1 dòng lỗi) — vì đây là dữ liệu incremental,
    /// 1 nhân viên lỗi không nên chặn 50 nhân viên khác cùng batch.
    /// </summary>
    public abstract class HrmSyncJob<TStaging, TEntity> : IHrmSyncJob
       where TStaging : class, IHrmStagingEntity
       where TEntity : class
    {
        protected readonly IUnitOfWork Uow;

        protected HrmSyncJob(IUnitOfWork uow) => Uow = uow;

        public abstract string EntityType { get; }
        public virtual int SyncOrder => 0;
        public virtual bool IsBlockingDependency => false;
        protected abstract Task<Dictionary<string, TEntity>> LoadExistingEntitiesAsync(
            List<string> keys, CancellationToken ct);

        protected abstract TEntity MapToNewEntity(TStaging staging);

        protected abstract bool ApplyUpdate(TEntity entity, TStaging staging);

        protected abstract bool ApplyDelete(TEntity entity);

        /// <summary>
        /// Chạy SAU khi toàn bộ batch đã qua Map/ApplyUpdate/ApplyDelete, TRƯỚC SaveChangesAsync —
        /// domain dùng để ghi entity phụ trợ (VD F03SyncReviewFlag) dựa trên những gì vừa
        /// đổi trong batch này, nằm CHUNG transaction với batch chính. Mặc định no-op.
        /// </summary>
        protected virtual Task AfterBatchAsync(HrmSyncBatchContext<TEntity> batchContext, CancellationToken ct)
            => Task.CompletedTask;

        public virtual async Task<HrmSyncResult> RunAsync(CancellationToken ct = default)
        {
            var result = new HrmSyncResult();

            var pending = await Uow.Repository<TStaging>().Query()
                .Where(x => !x.IsProcessed)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(ct);

            if (pending.Count == 0) return result;

            result.TotalSource = pending.Count;

            var groups = pending.GroupBy(x => x.EntityKey).ToList();

            var latest = groups
                .Select(g => g.OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x=>x.Id)
                .First())
                .ToList();

            var superseded = groups
                .SelectMany(g => g.OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x=>x.Id)
                .Skip(1))
                .ToList();

            var keys = latest.Select(x => x.EntityKey).ToList();
            var existingByKey = await LoadExistingEntitiesAsync(keys, ct);

            // Safety guard: never mass-deactivate FVN data because an HRM snapshot
            // is incomplete. Only HRM-owned rows may be deactivated automatically.
            var deleteItems = latest.Where(x => x.Action == HrmChangeAction.Delete).ToList();
            if (deleteItems.Count > 0)
            {
                var activeCount = await Uow.Repository<TEntity>().Query()
                    .CountAsync(x => EF.Property<bool?>(x, "IsActive") == true, ct);

                if (deleteItems.Count * 2 > Math.Max(activeCount, 1))
                {
                    foreach (var staging in deleteItems)
                    {
                        staging.IsProcessed = true;
                        staging.ErrorMessage = $"Delete guard: từ chối {deleteItems.Count} delete trên {activeCount} bản ghi active (>50%).";
                    }

                    result.Errors.Add($"Delete guard chặn {deleteItems.Count} bản ghi của {EntityType}: snapshot HRM có thể không đầy đủ.");
                    latest = latest.Where(x => x.Action != HrmChangeAction.Delete).ToList();
                }
            }

            var batchContext = new HrmSyncBatchContext<TEntity>();

            foreach (var staging in latest)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    existingByKey.TryGetValue(staging.EntityKey, out var entity);

                    switch (staging.Action)
                    {
                        case HrmChangeAction.Delete:
                            if (entity != null &&
                                string.Equals(EF.Property<string>(entity, "LastModifiedSource"), SyncSourceTags.Hrm, StringComparison.OrdinalIgnoreCase) &&
                                ApplyDelete(entity))
                            {
                                result.Deactivated++;
                                batchContext.Deleted.Add(entity);
                            }
                            break;

                        case HrmChangeAction.Insert:
                        case HrmChangeAction.Update:
                            if (entity == null)
                            {
                                var newEntity = MapToNewEntity(staging);
                                await Uow.Repository<TEntity>().AddAsync(newEntity, ct);
                                result.Added++;
                                batchContext.Added.Add(newEntity);
                            }
                            else
                            {
                                if (ApplyUpdate(entity, staging))
                                {
                                    result.Updated++;
                                    batchContext.Updated.Add(entity);
                                }
                                else
                                {
                                    result.Unchanged++;
                                }
                            }
                            break;
                    }

                    staging.IsProcessed = true;
                    staging.ErrorMessage = null;
                }
                catch (Exception ex)
                {
                    staging.ErrorMessage = ex.Message;
                    result.Errors.Add($"{staging.EntityKey}: {ex.Message}");
                }
            }

            foreach (var raw in superseded)
            {
                raw.IsProcessed = true;
                raw.ErrorMessage = "Superseded by newer change";
            }

            await AfterBatchAsync(batchContext, ct);

            await Uow.SaveChangesAsync(ct);
            return result;
        }
    }
}

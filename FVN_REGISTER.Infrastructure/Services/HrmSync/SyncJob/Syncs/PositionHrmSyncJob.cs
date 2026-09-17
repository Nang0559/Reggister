using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.BaseSyncJob;
using Microsoft.EntityFrameworkCore;
using static Dapper.SqlMapper;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.Syncs
{
    public class PositionHrmSyncJob : HrmSyncJob<F03StagingPosition, F03Position>
    {
        // Đánh dấu system user khi HrmSyncJob tự ghi — phân biệt với Admin sửa tay.
        // ⚠️ TODO tạm: thay bằng LastModifiedSource nếu bạn quyết định thêm field riêng sau này.
        
        private readonly List<(string Key, string OldName, string NewName)> _overwrittenManualEdits = new();
        private readonly List<string> _deletedButReferenced = new();
        public PositionHrmSyncJob(IUnitOfWork uow) : base(uow) { }

        public override string EntityType => "Position";
        public override bool IsBlockingDependency => true;
        protected override async Task<Dictionary<string, F03Position>> LoadExistingEntitiesAsync(
            List<string> keys, CancellationToken ct)
        {
            var list = await Uow.Repository<F03Position>().Query()
                .Where(x => keys.Contains(x.PositionCode))
                .ToListAsync(ct);
            return list.ToDictionary(x => x.PositionCode);
        }

        protected override F03Position MapToNewEntity(F03StagingPosition s) => new()
        {
            PositionCode = s.EntityKey,
            PositionName = s.PositionName,
            IsApprove = false,
            IsAllowApprove = false,
            DefaultApproveLevel = s.DefaultApproveLevel,
      
            LastModifiedSource = SyncSourceTags.Hrm
        };

        protected override bool ApplyUpdate(F03Position entity, F03StagingPosition staging)
        {
            bool changed = false;

            bool wasManuallyEdited = entity.LastModifiedSource == SyncSourceTags.Manual;
            string oldName = entity.PositionName;

            if (entity.PositionName != staging.PositionName)
            {
                entity.PositionName = staging.PositionName;
                changed = true;
            }
            if (entity.DefaultApproveLevel != staging.DefaultApproveLevel)
            {
                entity.DefaultApproveLevel = staging.DefaultApproveLevel;
                changed = true;
            }
            if (entity.IsActive != true)
            {
                entity.IsActive = true;   // xuất hiện lại trong nguồn HRM → kích hoạt lại
                changed = true;
            }

            // IsApprove/IsAllowApprove: KHÔNG BAO GIỜ bị Sync đụng vào — thuộc quyền Admin hoàn toàn.

            if (changed)
            {
                entity.LastModifiedSource = SyncSourceTags.Hrm;

                if (wasManuallyEdited)
                {
                    _overwrittenManualEdits.Add((entity.PositionCode, oldName, staging.PositionName));
                }
            }

            return changed;
        }

        protected override bool ApplyDelete(F03Position entity)
        {
            if (entity.IsActive != true) return false;

            entity.IsActive = false;
            entity.LastModifiedSource = SyncSourceTags.Hrm;

            // Position bị xoá bên HRM nhưng có thể đang được F03Employee tham chiếu —
            // ghi nhận để cảnh báo, KHÔNG chặn soft-delete (giữ IsActive=false vẫn an toàn cho FK).
            _deletedButReferenced.Add(entity.PositionCode);

            return true;
        }
        protected override async Task AfterBatchAsync(
            HrmSyncBatchContext<F03Position> batchContext, CancellationToken ct)
        {
            var flagRepo = Uow.Repository<F03SyncReviewFlag>();

            foreach (var (key, oldName, newName) in _overwrittenManualEdits)
            {
                await flagRepo.AddAsync(new F03SyncReviewFlag
                {
                    EntityType = EntityType,
                    EntityKey = key,
                    FlagType = "ManualEditOverwrittenByHrm",
                    Message = $"Chức vụ {key} vừa bị đồng bộ HRM ghi đè tên " +
                              $"(\"{oldName}\" → \"{newName}\"), trong khi trước đó đã được Admin sửa tay. " +
                              $"Cờ IsApprove/IsAllowApprove KHÔNG bị ảnh hưởng bởi lần đồng bộ này."
                }, ct);
            }

            if (_deletedButReferenced.Count == 0) return;

            var affectedEmployeeCounts = await Uow.Repository<F03Employee>().Query()
                .Where(e => e.IsActive == true && _deletedButReferenced.Contains(e.PositionCode))
                .GroupBy(e => e.PositionCode)
                .Select(g => new { PositionCode = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            foreach (var item in affectedEmployeeCounts)
            {
                await flagRepo.AddAsync(new F03SyncReviewFlag
                {
                    EntityType = EntityType,
                    EntityKey = item.PositionCode,
                    FlagType = "DeactivatedButStillReferenced",
                    Message = $"Chức vụ {item.PositionCode} đã bị vô hiệu hoá theo HRM, " +
                              $"nhưng vẫn đang có {item.Count} nhân viên active tham chiếu tới. " +
                              $"Cần review và cập nhật PositionCode cho các nhân viên này."
                }, ct);
            }
        }
    }
}


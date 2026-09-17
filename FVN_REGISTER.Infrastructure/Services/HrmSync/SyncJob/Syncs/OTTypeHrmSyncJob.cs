using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.BaseSyncJob;
using Microsoft.EntityFrameworkCore;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.Syncs
{
    public class OTTypeHrmSyncJob : HrmSyncJob<F03StagingOTType, F03OTType>
    {
        public OTTypeHrmSyncJob(IUnitOfWork uow) : base(uow) { }

        public override string EntityType => "OTType";

        protected override async Task<Dictionary<string, F03OTType>> LoadExistingEntitiesAsync(
            List<string> keys, CancellationToken ct)
        {
            var list = await Uow.Repository<F03OTType>().Query()
                .Where(x => keys.Contains(x.OTTypeCode))
                .ToListAsync(ct);
            return list.ToDictionary(x => x.OTTypeCode);
        }

        protected override F03OTType MapToNewEntity(F03StagingOTType s) => new()
        {
            OTTypeCode = s.EntityKey,
            OTTypeName = s.OTTypeName,
            OTTypeName2 = s.OTTypeName2,
            RateMultiplier = s.RateMultiplier,
            HRMCode = s.HRMCode
        };

        protected override bool ApplyUpdate(F03OTType e, F03StagingOTType s)
        {
            bool changed = false;
            if (e.OTTypeName != s.OTTypeName) { e.OTTypeName = s.OTTypeName; changed = true; }
            if (e.OTTypeName2 != s.OTTypeName2) { e.OTTypeName2 = s.OTTypeName2; changed = true; }
            if (e.RateMultiplier != s.RateMultiplier) { e.RateMultiplier = s.RateMultiplier; changed = true; }
            if (e.HRMCode != s.HRMCode) { e.HRMCode = s.HRMCode; changed = true; }
            if (e.IsActive != true) { e.IsActive = true; changed = true; }
            return changed;
        }

        protected override bool ApplyDelete(F03OTType e)
        {
            if (e.IsActive == false) return false;
            e.IsActive = false;
            return true;
        }
    }
}
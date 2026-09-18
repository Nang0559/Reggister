using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.BaseSyncJob;
using Microsoft.EntityFrameworkCore;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.Syncs
{
    public class LeaveTypeHrmSyncJob : HrmSyncJob<F03StagingLeaveType, F03LeaveType>
    {
        public LeaveTypeHrmSyncJob(IUnitOfWork uow) : base(uow) { }

        public override string EntityType => "LeaveType";

        protected override async Task<Dictionary<string, F03LeaveType>> LoadExistingEntitiesAsync(
            List<string> keys, CancellationToken ct)
        {
            var list = await Uow.Repository<F03LeaveType>().Query()
                .Where(x => keys.Contains(x.LeaveTypeCode))
                .ToListAsync(ct);
            return list.ToDictionary(x => x.LeaveTypeCode);
        }

        protected override F03LeaveType MapToNewEntity(F03StagingLeaveType s) => new()
        {
            LeaveTypeCode = s.EntityKey,
            LeaveTypeName = s.LeaveTypeName,
            LeaveTypeName2 = s.LeaveTypeName2,
            IsCountedAsLeave = s.TinhPhep,
            HRMCode = s.HRMCode
        };

        protected override bool ApplyUpdate(F03LeaveType e, F03StagingLeaveType s)
        {
            bool changed = false;
            if (e.LeaveTypeName != s.LeaveTypeName) { e.LeaveTypeName = s.LeaveTypeName; changed = true; }
            if (e.LeaveTypeName2 != s.LeaveTypeName2) { e.LeaveTypeName2 = s.LeaveTypeName2; changed = true; }
        
            if (e.HRMCode != s.HRMCode) { e.HRMCode = s.HRMCode; changed = true; }
            if (e.IsCountedAsLeave != s.TinhPhep) { e.IsCountedAsLeave = s.TinhPhep; changed = true; }
            if (e.IsActive != true) { e.IsActive = true; changed = true; }   // hồi phục nếu trước đó bị vô hiệu hóa
            return changed;
        }

        protected override bool ApplyDelete(F03LeaveType e)
        {
            if (e.IsActive == false) return false;
            e.IsActive = false;
            return true;
        }
    }
}

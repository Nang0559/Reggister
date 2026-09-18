using FVN_REGISTER.Core.Constants;

using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.BaseSyncJob;
using Microsoft.EntityFrameworkCore;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.Syncs
{
    public class DepartmentHrmSyncJob : HrmSyncJob<F03StagingDepartment, F03Department>
    {
        private readonly List<string> _deactivatedDeptCodes = new();
        // ★ MỚI — track để AfterBatchAsync ghi F03SyncReviewFlag
        private readonly List<string> _overwrittenManualEdits = new();          // đổi tên bị Hrm ghi đè
        private readonly List<string> _overwrittenManualDeactivations = new();  // Admin từng tắt tay, Hrm bật lại

        public DepartmentHrmSyncJob(IUnitOfWork uow) : base(uow) { }

        public override string EntityType => "Department";
        public override bool IsBlockingDependency => true;
        public override int SyncOrder => 0;

        protected override async Task<Dictionary<string, F03Department>> LoadExistingEntitiesAsync(
            List<string> keys, CancellationToken ct)
        {
            var list = await Uow.Repository<F03Department>().Query()
                .Where(x => keys.Contains(x.DeptCode))
                .ToListAsync(ct);
            return list.ToDictionary(x => x.DeptCode);
        }

        protected override F03Department MapToNewEntity(F03StagingDepartment s) => new()
        {
            DeptCode = s.EntityKey,
            DeptName = s.DeptName,
            IsActive = true,
            ParentDeptCode = s.ParentDeptCode,
            DisplayPriority = s.DisplayPriority,
            ShowInReport = s.ShowInReport,
            LastModifiedSource = SyncSourceTags.Hrm
        };

        // ===== TÁCH RIÊNG 2 case: đổi tên vs reactivate =====
        protected override bool ApplyUpdate(F03Department e, F03StagingDepartment s)
        {
            bool changed = false;
            bool wasManuallyEdited = e.LastModifiedSource == SyncSourceTags.Manual;

            // Case 1: đổi tên
            if (e.DeptName != s.DeptName)
            {
                if (wasManuallyEdited)
                    _overwrittenManualEdits.Add(e.DeptCode);

                e.DeptName = s.DeptName;
                changed = true;
            }

            if (e.ParentDeptCode != s.ParentDeptCode) { e.ParentDeptCode = s.ParentDeptCode; changed = true; }
            if (e.DisplayPriority != s.DisplayPriority) { e.DisplayPriority = s.DisplayPriority; changed = true; }
            if (e.ShowInReport != s.ShowInReport) { e.ShowInReport = s.ShowInReport; changed = true; }

            // Case 2: reactivate (Admin từng tắt tay, HRM báo vẫn active)
            if (e.IsActive != true)
            {
                if (wasManuallyEdited)
                    _overwrittenManualDeactivations.Add(e.DeptCode);

                e.IsActive = true;
                changed = true;
            }

            if (changed)
                e.LastModifiedSource = SyncSourceTags.Hrm;

            return changed;
        }

        protected override bool ApplyDelete(F03Department e)
        {
            if (e.IsActive == false) return false;

            e.IsActive = false;
            e.LastModifiedSource = SyncSourceTags.Hrm;
            _deactivatedDeptCodes.Add(e.DeptCode);
            return true;
        }

        protected override async Task AfterBatchAsync(
            HrmSyncBatchContext<F03Department> batchContext, CancellationToken ct)
        {
            var flagRepo = Uow.Repository<F03SyncReviewFlag>();

            // (a) ManualEditOverwrittenByHrm
            foreach (var code in _overwrittenManualEdits)
            {
                await flagRepo.AddAsync(new F03SyncReviewFlag
                {
                    EntityType = "Department",
                    EntityKey = code,
                    FlagType = "ManualEditOverwrittenByHrm",
                    Message = $"Tên phòng ban {code} do Admin sửa tay đã bị HRM ghi đè lại — cần review."
                }, ct);
            }

            // (b) ManualDeactivationOverwrittenByHrm
            foreach (var code in _overwrittenManualDeactivations)
            {
                await flagRepo.AddAsync(new F03SyncReviewFlag
                {
                    EntityType = "Department",
                    EntityKey = code,
                    FlagType = "ManualDeactivationOverwrittenByHrm",
                    Message = $"Phòng ban {code} đã bị Admin vô hiệu hóa tay, nhưng HRM vừa kích hoạt lại — cần review."
                }, ct);
            }

            // (c) DeactivatedButStillReferenced — giữ nguyên logic bạn đã viết
            if (_deactivatedDeptCodes.Count > 0)
            {
                var affectedApprovers = await Uow.Repository<F03Approver>().Query()
                    .Where(a => a.IsActive == true &&
                        (_deactivatedDeptCodes.Contains(a.ApproverDeptCode) ||
                         _deactivatedDeptCodes.Contains(a.ApproveForDeptCode)))
                    .ToListAsync(ct);

                foreach (var approver in affectedApprovers)
                {
                    await flagRepo.AddAsync(new F03SyncReviewFlag
                    {
                        EntityType = "Department",
                        EntityKey = approver.ApproveForDeptCode,
                        FlagType = "DeptDeactivated_ApproverMayBeStale",
                        Message = $"Phòng ban {approver.ApproveForDeptCode} vừa bị vô hiệu hóa từ HRM, " +
                                  $"nhưng approver {approver.ApproverCode} Level {approver.Level} " +
                                  $"vẫn đang cấu hình liên quan đến phòng này — cần review lại F03Approver."
                    }, ct);
                }
            }
        }
    }
}

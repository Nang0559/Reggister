using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.BaseSyncJob;
using Microsoft.EntityFrameworkCore;


namespace FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.Syncs
{
    public class EmployeeHrmSyncJob : HrmSyncJob<F03StagingEmployee, F03Employee>
    {
        private readonly List<string> _changedApproverRelevantCodes = new();
        private Dictionary<string, int?> _positionLevelCache = new();

        public EmployeeHrmSyncJob(IUnitOfWork uow) : base(uow) { }

        public override string EntityType => "Employee";
        public override int SyncOrder => 1;
        public override bool IsBlockingDependency => false;
        // Employee KHÔNG chặn job khác nếu lỗi — vì không có entity nào phụ thuộc FK
        // ngược vào Employee trong Pipeline A (ngược lại Employee mới là bên phụ thuộc
        // Department/Position, nên Department/Position mới cần IsBlockingDependency=true)

        protected override async Task<Dictionary<string, F03Employee>> LoadExistingEntitiesAsync(
            List<string> keys, CancellationToken ct)
        {
            var list = await Uow.Repository<F03Employee>().Query()
                .Where(x => keys.Contains(x.EmployeeCode))
                .ToListAsync(ct);

            _positionLevelCache = await Uow.Repository<F03Position>().Query()
                .Where(p => p.IsActive == true)
                .ToDictionaryAsync(p => p.PositionCode, p => p.DefaultApproveLevel, ct);

            return list.ToDictionary(x => x.EmployeeCode);
        }

        protected override F03Employee MapToNewEntity(F03StagingEmployee s) => new()
        {
            EmployeeCode = s.EntityKey,
            EmployeeName = s.EmployeeName,
            DeptCode = s.DeptCode ?? string.Empty,
            PositionCode = s.PositionCode ?? string.Empty,
            EmailAddress = s.EmailAddress,
            PhoneNumber = s.PhoneNumber,
            BirthDate = s.BirthDate,
            GenderCode = s.GenderCode,
            FirstWorkingDate = s.FirstWorkingDate,
            EndWorkingDate = s.EndWorkingDate,
            EmployeeNo = s.EmployeeNo,
            TotalLeaveDays = s.TotalLeaveDays ?? 0,
            LevelApprove = GetSuggestedLevel(s.PositionCode),
            IsActive = !s.EndWorkingDate.HasValue,
            LastModifiedSource = SyncSourceTags.Hrm   // ★ MỚI
        };

        protected override bool ApplyUpdate(F03Employee e, F03StagingEmployee s)
        {
            bool changed = false;

            if (e.EmployeeName != s.EmployeeName) { e.EmployeeName = s.EmployeeName; changed = true; }

            bool deptChanged = e.DeptCode != (s.DeptCode ?? string.Empty);
            if (deptChanged) { e.DeptCode = s.DeptCode ?? string.Empty; changed = true; }

            bool positionChanged = e.PositionCode != (s.PositionCode ?? string.Empty);
            if (positionChanged) { e.PositionCode = s.PositionCode ?? string.Empty; changed = true; }

            if (e.BirthDate != s.BirthDate) { e.BirthDate = s.BirthDate; changed = true; }
            if (e.GenderCode != s.GenderCode) { e.GenderCode = s.GenderCode; changed = true; }
            if (e.FirstWorkingDate != s.FirstWorkingDate) { e.FirstWorkingDate = s.FirstWorkingDate; changed = true; }
            if (e.EmployeeNo != s.EmployeeNo) { e.EmployeeNo = s.EmployeeNo; changed = true; }

            if (e.EmailAddress != (s.EmailAddress ?? "")) { e.EmailAddress = s.EmailAddress; changed = true; }
            if (e.PhoneNumber != s.PhoneNumber) { e.PhoneNumber = s.PhoneNumber; changed = true; }
            if (e.EndWorkingDate != s.EndWorkingDate) { e.EndWorkingDate = s.EndWorkingDate; changed = true; }
            if (e.TotalLeaveDays != (s.TotalLeaveDays ?? 0)) { e.TotalLeaveDays = s.TotalLeaveDays ?? 0; changed = true; }

            bool shouldBeActive = !s.EndWorkingDate.HasValue;
            if (e.IsActive != shouldBeActive) { e.IsActive = shouldBeActive; changed = true; }

            if (positionChanged)
            {
                var suggested = GetSuggestedLevel(s.PositionCode);
                if (e.LevelApprove != suggested) { e.LevelApprove = suggested; changed = true; }
            }

            if (positionChanged || deptChanged)
            {
                _changedApproverRelevantCodes.Add(e.EmployeeCode);
            }

            if (changed)
            {
                e.LastModifiedSource = SyncSourceTags.Hrm;   // ★ MỚI — chỉ set khi thực sự có thay đổi
            }

            return changed;
        }

        protected override bool ApplyDelete(F03Employee e)
        {
            if (e.IsActive == false) return false;

            e.IsActive = false;
            e.EndWorkingDate ??= DateTime.Now;
            e.LastModifiedSource = SyncSourceTags.Hrm;   // ★ MỚI

            _changedApproverRelevantCodes.Add(e.EmployeeCode);

            return true;
        }

        protected override async Task AfterBatchAsync(
            HrmSyncBatchContext<F03Employee> batchContext, CancellationToken ct)
        {
            if (_changedApproverRelevantCodes.Count == 0) return;

            var affectedApprovers = await Uow.Repository<F03Approver>().Query()
                .Where(a => a.IsActive == true && _changedApproverRelevantCodes.Contains(a.ApproverCode))
                .ToListAsync(ct);

            if (affectedApprovers.Count == 0) return;

            var flagRepo = Uow.Repository<F03SyncReviewFlag>();

            foreach (var approver in affectedApprovers)
            {
                await flagRepo.AddAsync(new F03SyncReviewFlag
                {
                    EntityType = "Employee",
                    EntityKey = approver.ApproverCode,
                    FlagType = "PositionOrDeptChanged_ApproverMayBeStale",
                    Message = $"Nhân viên {approver.ApproverCode} ({approver.ApproverName}) đang cấu hình là approver " +
                              $"Level {approver.Level} cho {approver.RequestType} - phòng {approver.ApproveForDeptCode}, " +
                              $"nhưng vừa có thay đổi Phòng ban/Chức vụ từ HRM. Các đơn ĐANG chờ duyệt không bị ảnh hưởng " +
                              $"(đã snapshot approver tại thời điểm tạo đơn) — cần review F03Approver TRƯỚC KHI có đơn MỚI dùng đến."
                }, ct);
            }
        }

        private int? GetSuggestedLevel(string? positionCode)
            => positionCode != null && _positionLevelCache.TryGetValue(positionCode, out var level) ? level : null;
    }
}

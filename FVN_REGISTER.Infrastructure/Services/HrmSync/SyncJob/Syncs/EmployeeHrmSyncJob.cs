using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.BaseSyncJob;
using FVN_REGISTER.Infrastructure.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.Syncs
{
    public class EmployeeHrmSyncJob : HrmSyncJob<F03StagingEmployee, F03Employee>
    {
        private readonly List<string> _changedApproverRelevantCodes = new();
        private readonly Dictionary<string, int?> _positionLevelCache = new();
        private readonly ISessionTerminationNotifier _sessionNotifier;

        public EmployeeHrmSyncJob(IUnitOfWork uow, ISessionTerminationNotifier sessionNotifier) : base(uow)
        {
            _sessionNotifier = sessionNotifier;
        }

        public override string EntityType => "Employee";
        public override int SyncOrder => 1;
        public override bool IsBlockingDependency => false;

        protected override async Task<Dictionary<string, F03Employee>> LoadExistingEntitiesAsync(
            List<string> keys, CancellationToken ct)
        {
            var list = await Uow.Repository<F03Employee>().Query()
                .Where(x => keys.Contains(x.EmployeeCode))
                .ToListAsync(ct);

            var positionLevels = await Uow.Repository<F03Position>().Query()
                .Where(p => p.IsActive == true)
                .ToDictionaryAsync(p => p.PositionCode, p => p.DefaultApproveLevel, ct);

            _positionLevelCache.Clear();
            foreach (var item in positionLevels)
                _positionLevelCache[item.Key] = item.Value;

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
            LastModifiedSource = SyncSourceTags.Hrm
        };

        protected override bool ApplyUpdate(F03Employee e, F03StagingEmployee s)
        {
            bool changed = false;

            bool deptChanged = e.DeptCode != (s.DeptCode ?? string.Empty);
            bool positionChanged = e.PositionCode != (s.PositionCode ?? string.Empty);

            if (e.EmployeeName != s.EmployeeName) { e.EmployeeName = s.EmployeeName; changed = true; }
            if (deptChanged) { e.DeptCode = s.DeptCode ?? string.Empty; changed = true; }
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
                _changedApproverRelevantCodes.Add(e.EmployeeCode);

            if (changed)
                e.LastModifiedSource = SyncSourceTags.Hrm;

            return changed;
        }

        protected override bool ApplyDelete(F03Employee e)
        {
            if (e.IsActive == false) return false;

            e.IsActive = false;
            e.EndWorkingDate ??= DateTime.Now;
            e.LastModifiedSource = SyncSourceTags.Hrm;
            _changedApproverRelevantCodes.Add(e.EmployeeCode);
            return true;
        }

        protected override async Task AfterBatchAsync(
            HrmSyncBatchContext<F03Employee> batchContext, CancellationToken ct)
        {
            await ProvisionUsersAsync(batchContext, ct);
            await RefreshApproverReviewFlagsAsync(ct);
        }

        private async Task ProvisionUsersAsync(
            HrmSyncBatchContext<F03Employee> batchContext,
            CancellationToken ct)
        {
            // Full reconciliation ở F03Users giúp tự phục hồi user bị thiếu và áp dụng
            // role rule mới mà không cần chờ Employee có một thay đổi khác.
            var employees = await Uow.Repository<F03Employee>().Query()
                .ToListAsync(ct);

            if (employees.Count == 0)
                return;

            var employeeCodes = employees.Select(x => x.EmployeeCode).ToList();

            var users = await Uow.Repository<F03User>().Query()
                .Where(x => employeeCodes.Contains(x.EmployeeCode))
                .ToListAsync(ct);

            var usersByCode = users.ToDictionary(x => x.EmployeeCode);

            foreach (var employee in employees)
            {
                try
                {
                    var permissionCode = await ResolvePermissionCodeAsync(
                        employee.DeptCode,
                        employee.PositionCode,
                        ct);

                    if (!usersByCode.TryGetValue(employee.EmployeeCode, out var user))
                    {
                        user = new F03User
                        {
                            EmployeeCode = employee.EmployeeCode,
                            FullName = employee.EmployeeName,
                            Password = EncryptUtils.MD5("Fcc@123"),
                            PermissionCode = permissionCode,
                            LevelApprove = employee.LevelApprove ?? 0,
                            DeptCode = employee.DeptCode,
                            Cvcode = employee.PositionCode,
                            IsActive = employee.IsActive,
                            LastModifiedSource = SyncSourceTags.Hrm
                        };

                        await Uow.Repository<F03User>().AddAsync(user, ct);
                        usersByCode[employee.EmployeeCode] = user;

                        if (employee.IsActive == true && !string.IsNullOrWhiteSpace(employee.EmailAddress))
                            await QueueRegistrationEmailAsync(employee, ct);
                    }
                    else
                    {
                        var wasActive = user.IsActive == true;

                        user.FullName = employee.EmployeeName;
                        user.DeptCode = employee.DeptCode;
                        user.Cvcode = employee.PositionCode;
                        user.LevelApprove = employee.LevelApprove ?? 0;
                        // SECURITY BOUNDARY:
                        // Existing FVN users keep their role/permissions. HRM only owns
                        // employee identity/master fields. The HRM role rule is used only
                        // when provisioning a brand-new F03User.
                        user.IsActive = employee.IsActive;
                        user.LastModifiedSource = SyncSourceTags.Hrm;

                        if (wasActive && employee.IsActive != true)
                        {
                            user.LockoutEndDate = DateTime.Now;

                            var sessions = await Uow.Repository<F03UserSession>().Query()
                                .Where(x => x.UserId == user.Id && x.IsActive == true)
                                .ToListAsync(ct);

                            foreach (var session in sessions)
                            {
                                session.IsActive = false;
                                session.RevokedAt = DateTime.Now;

                                if (!string.IsNullOrWhiteSpace(session.SignalRConnectionId))
                                {
                                    await _sessionNotifier.NotifyRevokedAsync(
                                        session.SignalRConnectionId,
                                        "Tài khoản đã bị khóa do nhân viên nghỉ việc.",
                                        ct);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Uow.Repository<F03SyncReviewFlag>().AddAsync(new F03SyncReviewFlag
                    {
                        EntityType = "Employee",
                        EntityKey = employee.EmployeeCode,
                        FlagType = "UserProvisioningFailed",
                        Message = $"Không thể đồng bộ tài khoản F03User cho nhân viên {employee.EmployeeCode}: {ex.Message}"
                    }, ct);
                }
            }
        }

        private async Task<int> ResolvePermissionCodeAsync(
            string? deptCode,
            string? positionCode,
            CancellationToken ct)
        {
            const string sql = @"
SELECT TOP (1)
    PermissionCode
FROM dbo.F03HrmUserRoleRules
WHERE IsActive = 1
  AND (DeptCode = @DeptCode OR DeptCode IS NULL)
  AND (PositionCode = @PositionCode OR PositionCode IS NULL)
ORDER BY
    CASE
        WHEN DeptCode = @DeptCode AND PositionCode = @PositionCode THEN 0
        WHEN DeptCode = @DeptCode AND PositionCode IS NULL THEN 1
        WHEN DeptCode IS NULL AND PositionCode = @PositionCode THEN 2
        ELSE 3
    END,
    Priority,
    Id;";

            var rows = await Uow.SqlQueryRawAsync<UserRoleRuleRow>(
                sql,
                ct,
                new SqlParameter("@DeptCode", (object?)deptCode ?? DBNull.Value),
                new SqlParameter("@PositionCode", (object?)positionCode ?? DBNull.Value));

            return rows.FirstOrDefault()?.PermissionCode ?? UserPermissionCodes.User;
        }

        private async Task QueueRegistrationEmailAsync(F03Employee employee, CancellationToken ct)
        {
            var email = employee.EmailAddress.Trim();
            if (email.Length == 0) return;

            var exists = await Uow.Repository<F03EmailQueue>().Query()
                .AnyAsync(x =>
                    x.ToEmail == email &&
                    x.TemplateCode == "HRM_USER_REGISTER" &&
                    x.Status != FVN_REGISTER.Core.Enums.EmailStatus.Sent,
                    ct);

            if (exists) return;

            var body = $"""
                <p>Xin chào <strong>{System.Net.WebUtility.HtmlEncode(employee.EmployeeName)}</strong>,</p>
                <p>Tài khoản FVN Register của bạn đã được tạo tự động từ hệ thống HRM.</p>
                <p><strong>Mã nhân viên:</strong> {System.Net.WebUtility.HtmlEncode(employee.EmployeeCode)}</p>
                <p><strong>Mật khẩu ban đầu:</strong> Fcc@123</p>
                <p>Vui lòng đăng nhập và đổi mật khẩu ngay sau lần đăng nhập đầu tiên.</p>
                """;

            await Uow.Repository<F03EmailQueue>().AddAsync(new F03EmailQueue
            {
                ToEmail = email,
                Subject = "FVN Register - Tài khoản đã được tạo",
                Body = body,
                TemplateCode = "HRM_USER_REGISTER",
                Status = FVN_REGISTER.Core.Enums.EmailStatus.Pending,
                MaxRetry = 3,
                LastModifiedSource = SyncSourceTags.Hrm
            }, ct);
        }

        private async Task RefreshApproverReviewFlagsAsync(CancellationToken ct)
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

        private sealed class UserRoleRuleRow
        {
            public int PermissionCode { get; set; }
        }
    }
}


using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Application.Rules;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.API.Services.Approvals
{
    public class LeaveApprovalProvider
        : BaseApprovalProvider<LeaveRequestSubject, LeaveApprovalProvider>,
          IApprovalProvider<LeaveRequestSubject>
    {
        public override RequestModule RequestType => RequestModule.Leave;

        private const int SubLeaderStep = 1;
        private const int ManagerStep = 2;
        private const int GmStep = 3;

        protected override IReadOnlyList<(int Level, string LevelName, string RoleName)> LevelDefs { get; } = new[]
        {
            (SubLeaderStep, "Sub-leader/Leader", "SubLeader"),
            (ManagerStep,   "Manager",            "Manager"),
            (GmStep,        "GM",                 "GM"),
        };

        public LeaveApprovalProvider(
            IUnitOfWork uow,
            IEmailService email,
            IApprovalNotificationService notification,
            IEmployeeUserResolver userResolver,
            ILogger<LeaveApprovalProvider> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, email, notification, userResolver, logger, options)
        {
        }

        protected override bool? ResolveRequired(int level, ApprovalBuildContext ctx)
        {
            bool isWorker = CvCodeRules.IsLv1Required(ctx.PositionCode);

            return level switch
            {
                SubLeaderStep => isWorker ? true : (bool?)null,
                ManagerStep => true,
                GmStep => true,
                _ => null
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // GET SUBJECT — dùng LeaveRequestSubject.From(), set thêm CvCode
        // ═══════════════════════════════════════════════════════════════

        public override async Task<LeaveRequestSubject?> GetSubjectAsync(
            int requestId, CancellationToken ct)
        {
            var entity = await _uow.Repository<F03LeaveDay>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct);

            if (entity == null) return null;

            var info = await GetEmployeeInfoAsync(entity.EmployeeCode, ct);

            var subject = LeaveRequestSubject.From(entity, info?.EmployeeName, info?.DeptCode);
            subject.PositionCode = info?.PositionCode; // 
            return subject;
        }

        public override async Task<List<LeaveRequestSubject>> GetSubjectsAsync(
            List<int> requestIds, CancellationToken ct)
        {
            var entities = await _uow.Repository<F03LeaveDay>()
                .Query()
                .AsNoTracking()
                .Where(x => requestIds.Contains(x.Id) && x.IsActive == true)
                .ToListAsync(ct);

            var empCodes = entities.Select(e => e.EmployeeCode).Distinct().ToList();

            var empMap = await _uow.Repository<F03Employee>()
                .Query()
                .AsNoTracking()
                .Where(e => empCodes.Contains(e.EmployeeCode))
                .Select(e => new { e.EmployeeCode, e.EmployeeName, e.DeptCode, e.PositionCode })
                .ToDictionaryAsync(e => e.EmployeeCode, e => e, ct);

            return entities.Select(x =>
            {
                empMap.TryGetValue(x.EmployeeCode, out var info);
                var subject = LeaveRequestSubject.From(x, info?.EmployeeName, info?.DeptCode);
                subject.PositionCode = info?.PositionCode;
                return subject;
            }).ToList();
        }

        // ═══════════════════════════════════════════════════════════════
        // APPLY OVERALL STATUS
        // ═══════════════════════════════════════════════════════════════

        public override async Task ApplyOverallStatusAsync(
            int requestId,
            IReadOnlyList<ApprovalStepCalculatedDto> allSteps,
            CancellationToken ct)
        {
            var entity = await _uow.Repository<F03LeaveDay>()
                .Query()
                .FirstOrDefaultAsync(x => x.Id == requestId, ct);

            if (entity == null)
            {
                Logger.LogWarnIf(Debug,
                    "[LEAVE-PROVIDER] ApplyOverallStatus: không tìm thấy LeaveId={Id}", requestId);
                return;
            }

            var newStatus = ComputeOverallStatus(allSteps);

            Logger.LogDebugIf(Debug,
                "[LEAVE-PROVIDER] LeaveId={Id}: {Old} → {New}",
                requestId, entity.RequestStatus, newStatus);

            entity.RequestStatus = newStatus; // ApprovalStatus enum, không phải string
            entity.ModifiedAt = DateTime.Now;

            await _uow.SaveChangesAsync(ct);
        }

        private static ApprovalStatus ComputeOverallStatus(IReadOnlyList<ApprovalStepCalculatedDto> allSteps)
        {
            var required = allSteps.Where(s => s.IsRequired).OrderBy(s => s.Level).ToList();

            if (required.Any(s => s.Status == DecisionType.Rejected))
                return ApprovalStatus.Rejected;

            if (required.Count > 0 && required.All(s => s.Status == DecisionType.Approved))
                return ApprovalStatus.Approved;

            bool anyApproved = required.Any(s => s.Status == DecisionType.Approved);
            return anyApproved ? ApprovalStatus.InProgress : ApprovalStatus.Pending;
        }

        // ═══════════════════════════════════════════════════════════════
        // NOTIFY
        // ═══════════════════════════════════════════════════════════════

        public override async Task NotifyStepCompletedAsync(
        LeaveRequestSubject subject,
        ApprovalStepCalculatedDto completedStep,
        bool isFullyApproved,
        CancellationToken ct)
        {
            try
            {
                if (isFullyApproved)
                {
                    await NotifyEmployeeAsync(subject, ApprovalStatus.Approved, ct);
                    await NotifyEmployeeInAppAsync(subject, ApprovalStatus.Approved, ct);   // THÊM
                    Logger.LogInfoIf(Debug, "[LEAVE-PROVIDER] APPROVED: LeaveId={Id}", subject.RequestId);
                    return;
                }

                if (completedStep.Status == DecisionType.Rejected)
                {
                    await NotifyEmployeeAsync(subject, ApprovalStatus.Rejected, ct);
                    await NotifyEmployeeInAppAsync(subject, ApprovalStatus.Rejected, ct);   // THÊM
                    Logger.LogInfoIf(Debug, "[LEAVE-PROVIDER] REJECTED: LeaveId={Id} Level={Lv}",
                        subject.RequestId, completedStep.Level);
                    return;
                }

                Logger.LogDebugIf(Debug,
                    "[LEAVE-PROVIDER] Level {Lv} approved, chờ cấp tiếp theo", completedStep.Level);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "[LEAVE-PROVIDER] NotifyStepCompleted lỗi: LeaveId={Id}", subject.RequestId);
            }
        }

        private async Task NotifyEmployeeAsync(
        LeaveRequestSubject subject, ApprovalStatus status, CancellationToken ct)
        {
            var empEmail = await GetEmployeeEmailAsync(subject.EmployeeCode, ct);
            if (string.IsNullOrWhiteSpace(empEmail)) return;

            var templateCode = status == ApprovalStatus.Approved ? "LEAVE_APPROVED" : "LEAVE_REJECTED";

            await _email.QueueEmail(empEmail, templateCode, new
            {
                subject.RequestId,
                subject.EmployeeCode,
                StartDate = subject.StartDate.ToString("dd/MM/yyyy"),
                EndDate = subject.EndDate.ToString("dd/MM/yyyy"),
                TotalDay = subject.TotalDay,
                Status = status.ToDisplayName()
            }, ct);
        }

        // ═══════════════════════════════════════════════════════════════
        // TO PENDING ITEM
        // ═══════════════════════════════════════════════════════════════

        public override async Task<PendingApprovalItemDto> ToPendingItemAsync(
            LeaveRequestSubject subject,
            List<ApprovalStepCalculatedDto> steps,
            bool canApprove,
            CancellationToken ct)
        {
            string? deptName = null;
            if (!string.IsNullOrEmpty(subject.DeptCode))
            {
                deptName = await _uow.Repository<F03Department>()
                    .Query()
                    .AsNoTracking()
                    .Where(d => d.DeptCode == subject.DeptCode)
                    .Select(d => d.DeptName)
                    .FirstOrDefaultAsync(ct);
            }

            return new PendingApprovalItemDto
            {
                RequestId = subject.RequestId,
                Kind = subject.Module,
                EmployeeCode = subject.EmployeeCode,
                EmployeeName = subject.EmployeeName ?? string.Empty,
                DeptCode = subject.DeptCode ?? string.Empty,
                DeptName = deptName ?? string.Empty,
                FromDate = subject.StartDate,
                ToDate = subject.EndDate,
                TotalUnits = subject.TotalDay,
                ApprovalSteps = steps,
                CanApprove = canApprove
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════

        private async Task<(string? EmployeeName, string? DeptCode, string? PositionCode)?> GetEmployeeInfoAsync(
            string employeeCode, CancellationToken ct)
        {
            var emp = await _uow.Repository<F03Employee>()
                .Query()
                .AsNoTracking()
                .Where(e => e.EmployeeCode == employeeCode)
                .Select(e => new { e.EmployeeName, e.DeptCode, e.PositionCode })
                .FirstOrDefaultAsync(ct);

            return emp == null ? null : (emp.EmployeeName, emp.DeptCode, emp.PositionCode);
        }

        private async Task<string?> GetEmployeeEmailAsync(string employeeCode, CancellationToken ct)
        {
            return await _uow.Repository<F03Employee>()
                .Query()
                .AsNoTracking()
                .Where(e => e.EmployeeCode == employeeCode)
                .Select(e => e.EmailAddress)
                .FirstOrDefaultAsync(ct);
        }
    }
}

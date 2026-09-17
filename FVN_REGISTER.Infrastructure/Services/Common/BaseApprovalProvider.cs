
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Common
{
    public abstract class BaseApprovalProvider<TSubject, TProvider>
        : BaseService<TProvider>
        where TSubject : IApprovalSubject
        where TProvider : class
    {
        protected readonly IUnitOfWork _uow;
        protected readonly IEmailService _email;
        protected readonly IApprovalNotificationService _notification;  
        protected readonly IEmployeeUserResolver _userResolver;
        protected BaseApprovalProvider(
            IUnitOfWork uow,
            IEmailService email,
            IApprovalNotificationService notification,                   
        IEmployeeUserResolver userResolver,
            ILogger<TProvider> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
            _email = email;
            _notification = notification;                                 
            _userResolver = userResolver;
        }

        public abstract RequestModule RequestType { get; }
        protected abstract IReadOnlyList<(int Level, string LevelName, string RoleName)> LevelDefs { get; }

        protected abstract bool? ResolveRequired(int level, ApprovalBuildContext ctx);

        // ═══════════════════════════════════════════════════════════════
        // BUILD HIERARCHY (danh sách bước duyệt theo cấp)
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<ApprovalStepSnapshotDto>> BuildHierarchyAsync(
            ApprovalBuildContext ctx, CancellationToken ct)
        {
            var snapshotSteps = new List<ApprovalStepSnapshotDto>();

            foreach (var def in LevelDefs)
            {
                var required = ResolveRequired(def.Level, ctx);
                if (required == null) continue;

                var approver = await GetApproverForLevelAsync(def.Level, ctx.DeptCode, ct);
                if (approver == null)
                {
                    Logger.LogWarnIf(Debug, "[{Provider}] Không tìm được approver Level {Lv} dept={Dept}",
                        typeof(TProvider).Name, def.Level, ctx.DeptCode);
                    continue;
                }

                snapshotSteps.Add(new ApprovalStepSnapshotDto(
                    Level: def.Level,
                    LevelName: def.LevelName,
                    RoleName: def.RoleName,
                    ApproverCode: approver.ApproverCode,
                    ApproverName: approver.ApproverName,
                    ApproverEmail: approver.ApproverEmail,
                    IsRequired: required.Value
                ));
            }

            return snapshotSteps;
        }

        // ═══════════════════════════════════════════════════════════════
        // BUILD SNAPSHOT — mặc định dùng chung; domain nào cần khác thì override (virtual)
        // ⚠️ ĐÃ SỬA: bỏ RequestType.ToCode() khỏi tham số — Module được set ở
        // ApprovalEngine.InitializeStepsAsync (Module = Module), không thuộc snapshot DTO.
        // Cần xác nhận lại đúng thứ tự tham số của record ApprovalSnapshotDto.
        // ═══════════════════════════════════════════════════════════════
        public virtual async Task<ApprovalSnapshotDto> BuildSnapshotAsync(
        TSubject subject, ApprovalBuildContext ctx, CancellationToken ct)
        {
            var steps = await BuildHierarchyAsync(ctx, ct);

            return new ApprovalSnapshotDto(
                RequestId: subject.RequestId,
                ModuleName: RequestType,   // ✅ giữ lại, DTO yêu cầu tham số này
                CapturedAt: DateTime.Now,
                Steps: steps
            );
        }

        // ═══════════════════════════════════════════════════════════════
        // TÌM APPROVER THEO LEVEL — ưu tiên đúng phòng ban, fallback "ALL"
        // ═══════════════════════════════════════════════════════════════
        protected async Task<F03Approver?> GetApproverForLevelAsync(int level, string deptCode, CancellationToken ct)
        {
            

            var baseQuery = _uow.Repository<F03Approver>()
                .Query()
                .AsNoTracking()
                .Where(x => x.RequestType == RequestType && x.Level == level && x.IsActive == true);

            return await baseQuery.FirstOrDefaultAsync(x => x.ApproveForDeptCode == deptCode, ct)
                ?? await baseQuery.FirstOrDefaultAsync(x => x.ApproveForDeptCode == ApproveForDept.All, ct);
        }
        // ═══════════════════════════════════════════════════════════════
        // THÊM: NOTIFY IN-APP CHO NGƯỜI TẠO ĐƠN — DÙNG CHUNG MỌI DOMAIN
        // Domain con (OT/Leave/Trip...) gọi hàm này trong NotifyStepCompletedAsync
        // của chính nó, KHÔNG cần viết lại logic resolve UserId mỗi nơi.
        // ═══════════════════════════════════════════════════════════════
        protected async Task NotifyEmployeeInAppAsync(
            TSubject subject, ApprovalStatus status, CancellationToken ct)
        {
            var creatorUserId = await _userResolver.ResolveUserIdAsync(subject.EmployeeCode, ct);
            if (creatorUserId is null or <= 0)
            {
                Logger.LogWarnIf(Debug,
                    "[{Provider}] Bỏ qua in-app: không tìm thấy UserId cho {Code} | RequestId={Id}",
                    typeof(TProvider).Name, subject.EmployeeCode, subject.RequestId);
                return;
            }

            await _notification.NotifyCreatorInAppAsync(
                creatorUserId: creatorUserId.Value,
                creatorEmployeeCode: subject.EmployeeCode,
                status: status.ToString(),
                requestId: subject.RequestId,
                requestType: RequestType,
                ct: ct);
        }
        public abstract Task<TSubject?> GetSubjectAsync(int requestId, CancellationToken ct);
        public abstract Task<List<TSubject>> GetSubjectsAsync(List<int> requestIds, CancellationToken ct);
        public abstract Task ApplyOverallStatusAsync(int requestId, IReadOnlyList<ApprovalStepCalculatedDto> allSteps, CancellationToken ct);
        public abstract Task NotifyStepCompletedAsync(TSubject subject, ApprovalStepCalculatedDto completedStep, bool isFullyApproved, CancellationToken ct);
        public abstract Task<PendingApprovalItemDto> ToPendingItemAsync(
            TSubject subject, List<ApprovalStepCalculatedDto> steps, bool canApprove, CancellationToken ct);
    }
}


using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Approvals;

using FVN_REGISTER.Core.Constants;

using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Infrastructure.Services.Histories
{
    public class ApprovalHistoryService : BaseService<ApprovalHistoryService>, IApprovalHistoryService
    {
        private readonly IUnitOfWork _uow;

        public ApprovalHistoryService(
            IUnitOfWork uow,
            ILogger<ApprovalHistoryService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
        }

        // ════════════════════════════════════════════════════════════
        // 1. GHI NHẬN HÀNH ĐỘNG (Duyệt/Từ chối) — APPEND-ONLY
        // ════════════════════════════════════════════════════════════
        public async Task RecordActionAsync(ApprovalActionDto action, CancellationToken ct)
        {
            if (action.RequestIds == null || action.RequestIds.Count == 0)
            {
                Logger.LogWarnIf(Debug, "[APPROVAL_HISTORY] RecordActionAsync: RequestIds rỗng");
                return;
            }

            // 1. Load snapshot + steps liên quan để resolve StepId thật
            //    (F03ApprovalStepSnapshot không có RequestId trực tiếp,
            //     phải join qua F03ApprovalSnapshot.RequestId)
            var snapshots = await _uow.Repository<F03ApprovalSnapshot>().Query()
                .AsNoTracking()
                .Where(s => action.RequestIds.Contains(s.RequestId) && s.RequestType == action.Kind)
                .Include(s => s.Steps)
                .ToListAsync(ct);

            var stepLookup = snapshots
                .Select(s => new
                {
                    s.RequestId,
                    Step = s.Steps.FirstOrDefault(st => st.Level == action.Level)
                })
                .Where(x => x.Step != null)
                .ToDictionary(x => x.RequestId, x => x.Step!);

            // 2. Resolve ApproverName — DTO chỉ có ApproverCode
            var approverName = await ResolveApproverNameAsync(action.ApproverCode, ct);

            // 3. Xác định có phải Admin duyệt thay (override) hay không:
            //    override khi người thực hiện KHÔNG PHẢI approver được gán ở step đó
            bool isAdminActing = action.ApproverPermission == UserPermissionCodes.SuperAdmin
                               || action.ApproverPermission == UserPermissionCodes.Admin;

            var decision = action.IsReject ? DecisionType.Rejected : DecisionType.Approved;
            var now = DateTime.UtcNow;

            var repo = _uow.Repository<F03ApprovalHistory>();
            int skipped = 0;

            foreach (var requestId in action.RequestIds)
            {
                if (!stepLookup.TryGetValue(requestId, out var step))
                {
                    Logger.LogWarnIf(Debug,
                        "[APPROVAL_HISTORY] Không tìm thấy StepSnapshot cho RequestId={RequestId}, Level={Level}, Kind={Kind} — bỏ qua",
                        requestId, action.Level, action.Kind);
                    skipped++;
                    continue;
                }

                bool isOverride = isAdminActing && step.ApproverCode != action.ApproverCode;

                var history = new F03ApprovalHistory
                {
                    RequestType = action.Kind,
                    RequestId = requestId,
                    StepId = step.Id, // ĐÃ SỬA: FK thật vào F03ApprovalStepSnapshot.Id
                    ApproverCode = action.ApproverCode,
                    ApproverName = approverName,
                    Decision = decision,
                    Comment = action.Comment,
                    ActionAt = now,
                    IsOverriddenByAdmin = isOverride,
                    OverriddenByCode = isOverride ? action.ApproverCode : null,
                    OverriddenByName = isOverride ? approverName : null,
                    OverriddenAt = isOverride ? now : null,
                };

                await repo.AddAsync(history, ct);
            }

            await _uow.SaveChangesAsync(ct);

            Logger.LogInfoIf(Debug,
                "[APPROVAL_HISTORY] Recorded {Count} action(s), skipped {Skipped}",
                action.RequestIds.Count - skipped, skipped);
        }

        // ════════════════════════════════════════════════════════════
        // 2. LOG TRẠNG THÁI BAN ĐẦU — "Đã nộp đơn" trong timeline
        // ════════════════════════════════════════════════════════════
        public async Task LogInitialStatusAsync(int requestId, RequestModule module, CancellationToken ct)
        {
            var history = new F03ApprovalHistory
            {
                RequestType = module,
                RequestId = requestId,
                StepId = 0, // Sentinel: không gắn với step cụ thể nào — mốc "khởi tạo"
                ApproverCode = "SYSTEM",
                ApproverName = "System Generated",
                Decision = DecisionType.Pending, // ⚠️ xác nhận: DecisionType có giá trị này không?
                Comment = "Request submitted",
                ActionAt = DateTime.UtcNow,
            };

            await _uow.Repository<F03ApprovalHistory>().AddAsync(history, ct);
            await _uow.SaveChangesAsync(ct);

            Logger.LogDebugIf(Debug,
                "[APPROVAL_HISTORY] Logged initial status: RequestId={RequestId}, Module={Module}",
                requestId, module);
        }

        // ════════════════════════════════════════════════════════════
        // 3. LẤY LỊCH SỬ — đọc thuần, dùng cho UI timeline
        // ════════════════════════════════════════════════════════════
        public async Task<List<F03ApprovalHistory>> GetHistoryAsync(
    int requestId, RequestModule module, CancellationToken ct)
        {
            return await _uow.Repository<F03ApprovalHistory>().Query()
                .AsNoTracking()
                .Where(x => x.RequestId == requestId && x.RequestType == module)
                .OrderBy(x => x.StepId)
                .ThenBy(x => x.ActionAt)
                .ToListAsync(ct);
        }

        // ════════════════════════════════════════════════════════════
        // HELPER: resolve ApproverName từ ApproverCode
        // ════════════════════════════════════════════════════════════
        private async Task<string> ResolveApproverNameAsync(string approverCode, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(approverCode)) return "Unknown";

            var name = await _uow.Repository<F03Employee>().Query()
                .AsNoTracking()
                .Where(e => e.EmployeeCode == approverCode)
                .Select(e => e.EmployeeName)
                .FirstOrDefaultAsync(ct);

            return name ?? approverCode;
        }
    }
}


using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Approvals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Leaves
{
    public class LeaveEscalationService
           : ApprovalEscalationService<LeaveRequestSubject>, ILeaveEscalationService
    {
        protected override RequestModule ModuleKind => RequestModule.Leave;

        public LeaveEscalationService(
            IUnitOfWork uow,
            IApprovalProvider<LeaveRequestSubject> provider,
            IEscalationRuleService rule,
            IWorkingDayService workingDay,
            IEmailService email,
            ILogger<ApprovalEscalationService<LeaveRequestSubject>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, provider, rule, workingDay, email, logger, options)
        {
        }

        protected override async Task<List<int>> GetActiveRequestIdsAsync(CancellationToken ct)
        {
            // Chỉ escalate các đơn còn đang trong luồng duyệt (chưa Approved/Rejected/Cancelled)
            return await Uow.Repository<F03LeaveDay>().Query()
                .Where(x => x.IsActive == true
                         && x.RequestStatus != ApprovalStatus.Approved
                         && x.RequestStatus != ApprovalStatus.Rejected
                         && x.RequestStatus != ApprovalStatus.Cancelled)
                .Select(x => x.Id)
                .ToListAsync(ct);
        }

        protected override async Task<Dictionary<int, DateTime>> GetRegisterDateMapAsync(
            List<int> ids, CancellationToken ct)
        {
            // GIẢ ĐỊNH: F03LeaveDay không có cột RegisterDate riêng (bản mới chỉ có
            // RegisterId, kế thừa CreatedAt từ BaseAuditEntity) — dùng CreatedAt làm
            // mốc "ngày đăng ký" để tính deadline cấp 1. Xác nhận lại nếu RegisterDate
            // là 1 cột riêng bạn chưa gửi, hoặc RegisterId trỏ tới bảng khác có ngày thật.
            return await Uow.Repository<F03LeaveDay>().Query()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.CreatedAt, ct);
        }

        protected override async Task<Dictionary<int, string?>> GetDeptCodeMapAsync(
            List<int> ids, CancellationToken ct)
        {
            return await Uow.Repository<F03LeaveDay>().Query()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DeptCode, ct);
        }
    }
}

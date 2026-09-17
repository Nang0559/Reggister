
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.OT;

using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Services.Approvals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Infrastructure.Services.OT
{
    public class OTEscalationService
         : ApprovalEscalationService<OTRequestSubject>, IOTEscalationService
    {
        protected override RequestModule ModuleKind => RequestModule.Overtime;

        public OTEscalationService(
            IUnitOfWork uow,
            IApprovalProvider<OTRequestSubject> provider,
            IEscalationRuleService rule,
            IWorkingDayService workingDay,
            IEmailService email,
            ILogger<ApprovalEscalationService<OTRequestSubject>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, provider, rule, workingDay, email, logger, options)
        {
        }

        protected override async Task<List<int>> GetActiveRequestIdsAsync(CancellationToken ct)
        {
            return await Uow.Repository<F03OTRequest>().Query()
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
            // GIẢ ĐỊNH tương tự Leave — dùng CreatedAt làm mốc bắt đầu tính deadline.
            // Cân nhắc: OT có thể nên dùng OTDate (ngày làm thêm thật) thay vì CreatedAt
            // (ngày tạo đơn) — 2 mốc này có thể lệch nhau nếu đăng ký OT trước ngày làm.
            // Xác nhận nghiệp vụ: escalation tính từ lúc TẠO ĐƠN hay từ NGÀY OT THẬT?
            return await Uow.Repository<F03OTRequest>().Query()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.CreatedAt, ct);
        }

        protected override async Task<Dictionary<int, string?>> GetDeptCodeMapAsync(
            List<int> ids, CancellationToken ct)
        {
            return await Uow.Repository<F03OTRequest>().Query()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DeptCode, ct);
        }
    }
}

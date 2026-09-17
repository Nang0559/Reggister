using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Core.Enums;
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
            IApprovalNotificationService notification,
            IEmployeeUserResolver userResolver,
            ILogger<ApprovalEscalationService<OTRequestSubject>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, provider, rule, workingDay, email, notification, userResolver, logger, options)
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

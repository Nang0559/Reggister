using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Services.Approvals
{
    /// <summary>
    /// Cross-module resolver. API/Dashboard never need to know which concrete
    /// approval engine/provider handles Leave or Overtime.
    /// </summary>
    public sealed class ApprovalEngineResolver : IApprovalEngineResolver
    {
        private readonly IApprovalEngine<LeaveRequestSubject> _leave;
        private readonly IApprovalEngine<OTRequestSubject> _ot;

        public ApprovalEngineResolver(
            IApprovalEngine<LeaveRequestSubject> leave,
            IApprovalEngine<OTRequestSubject> ot)
        {
            _leave = leave;
            _ot = ot;
        }

        public Task<ApprovalActionResult> ProcessDecisionAsync(
            RequestModule module,
            ApprovalActionDto action,
            CancellationToken ct)
        {
            return module switch
            {
                RequestModule.Leave => _leave.ProcessDecisionAsync(action, ct),
                RequestModule.Overtime => _ot.ProcessDecisionAsync(action, ct),
                _ => throw new NotSupportedException($"Approval module {module} chưa được hỗ trợ.")
            };
        }

        public Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(
            RequestModule module,
            string approverEmail,
            CancellationToken ct)
        {
            return module switch
            {
                RequestModule.Leave => _leave.GetPendingForApproverAsync(approverEmail, ct),
                RequestModule.Overtime => _ot.GetPendingForApproverAsync(approverEmail, ct),
                _ => throw new NotSupportedException($"Approval module {module} chưa được hỗ trợ.")
            };
        }
    }
}

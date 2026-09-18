
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Responses;


namespace FVN_REGISTER.Infrastructure.Services.Approvals;

public sealed class ApprovalEngineResolver : IApprovalEngineResolver
{
    private readonly IApprovalEngine<LeaveRequestSubject> _leave;
    private readonly IApprovalEngine<OTRequestSubject> _ot;
    private readonly IApprovalEngine<TripRequestSubject> _trip;
    private readonly IApprovalEngine<EquipmentRequestSubject> _equipment;

    public ApprovalEngineResolver(IApprovalEngine<LeaveRequestSubject> leave, IApprovalEngine<OTRequestSubject> ot,
        IApprovalEngine<TripRequestSubject> trip, IApprovalEngine<EquipmentRequestSubject> equipment)
    { _leave = leave; _ot = ot; _trip = trip; _equipment = equipment; }

    public Task<ApprovalActionResult> ProcessDecisionAsync(RequestModule module, ApprovalActionDto action, CancellationToken ct)
        => module switch
        {
            RequestModule.Leave => _leave.ProcessDecisionAsync(action, ct),
            RequestModule.Overtime => _ot.ProcessDecisionAsync(action, ct),
            RequestModule.Trip => _trip.ProcessDecisionAsync(action, ct),
            RequestModule.Equipment => _equipment.ProcessDecisionAsync(action, ct),
            _ => throw new NotSupportedException($"Approval module {module} chưa được hỗ trợ.")
        };

    public Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(RequestModule module, string approverEmail, CancellationToken ct)
        => module switch
        {
            RequestModule.Leave => _leave.GetPendingForApproverAsync(approverEmail, ct),
            RequestModule.Overtime => _ot.GetPendingForApproverAsync(approverEmail, ct),
            RequestModule.Trip => _trip.GetPendingForApproverAsync(approverEmail, ct),
            RequestModule.Equipment => _equipment.GetPendingForApproverAsync(approverEmail, ct),
            _ => throw new NotSupportedException($"Approval module {module} chưa được hỗ trợ.")
        };
}

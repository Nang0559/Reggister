using FVN_REGISTER.Contract.Models;


namespace FVN_REGISTER.Contract.Interfaces.Leaves
{
    public interface IEscalationRuleService
    {
        Task<int> GetTimeoutDaysAsync(F03leaveDay leave, int level, string deptCode, CancellationToken ct);
    }
}

using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Histories
{
    public interface IApprovalHistoryService
    {
        Task<List<F03ApprovalHistory>> GetHistoryAsync(int requestId, RequestModule module, CancellationToken ct);
    }
}
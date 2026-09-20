using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Approvals;

public interface IApprovalSelectionService
{
    Task ReplaceAsync(
        RequestModule requestType,
        int requestId,
        IEnumerable<ApprovalSelectionDto> selections,
        int createdBy,
        CancellationToken ct = default);

    Task<List<ApprovalSelectionDto>> GetAsync(
        RequestModule requestType,
        int requestId,
        CancellationToken ct = default);

}

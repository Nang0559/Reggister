using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Shared.Services.Approvals
{
    public interface IApprovalListClientService
    {
        Task<ApiResponse<List<PendingApprovalGroupDto>>> GetPendingAsync(
            CancellationToken ct = default);

        Task<ApiResponse<object>> ApproveAsync(
            List<int> ids,
            RequestModule kind,
            int level,
            string? comment,
            CancellationToken ct = default);

        Task<ApiResponse<object>> RejectAsync(
            List<int> ids,
            RequestModule kind,
            int level,
            string comment,
            CancellationToken ct = default);
    }
}

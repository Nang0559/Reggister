



using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Approvals
{
    // Interface
    public interface IApprovalListClientService
    {
        Task<ApiResponse<PendingApprovalListDto>> GetPendingAsync(
         CancellationToken ct = default);

        Task<ApiResponse<object>> ApproveAsync(
            List<int> ids,
            RequestKind kind,
            int level,
            string? comment,
            CancellationToken ct = default);

        Task<ApiResponse<object>> RejectAsync(
            List<int> ids,
            RequestKind kind,
            int level,
            string comment,
            CancellationToken ct = default);
    }

   
   
}

using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces.Repositores;



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

using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Approvals;

public interface IApprovalRouteClientService
{
    Task<ApiResponse<ApprovalRoutePreviewDto>> GetPreviewAsync(
        RequestModule requestType,
        CancellationToken ct = default);
}

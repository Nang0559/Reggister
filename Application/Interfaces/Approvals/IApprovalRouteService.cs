using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Approvals;

public interface IApprovalRouteService
{
    Task<ApprovalRoutePreviewDto> GetPreviewAsync(
        RequestModule requestType,
        string employeeCode,
        string deptCode,
        string positionCode,
        CancellationToken ct = default);
}

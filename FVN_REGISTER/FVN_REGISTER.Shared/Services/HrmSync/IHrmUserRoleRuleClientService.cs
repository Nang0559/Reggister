using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Dtos.Positions;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Requests.HrmSync;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.HrmSync;

public interface IHrmUserRoleRuleClientService
{
    Task<ApiResponse<List<HrmUserRoleRuleDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync(CancellationToken ct = default);
    Task<ApiResponse<List<PositionDto>>> GetPositionsAsync(CancellationToken ct = default);
    Task<ApiResponse<HrmUserRoleRuleDto>> CreateAsync(HrmUserRoleRuleRequest request, CancellationToken ct = default);
    Task<ApiResponse<HrmUserRoleRuleDto>> UpdateAsync(int id, HrmUserRoleRuleRequest request, CancellationToken ct = default);
    Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default);
}
using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Requests.HrmSync;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Application.Interfaces.HrmSync;

public interface IHrmUserRoleRuleService
{
    Task<ServiceResult<List<HrmUserRoleRuleDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<HrmUserRoleRuleDto>> CreateAsync(HrmUserRoleRuleRequest request, int actorUserId, CancellationToken ct = default);
    Task<ServiceResult<HrmUserRoleRuleDto>> UpdateAsync(int id, HrmUserRoleRuleRequest request, int actorUserId, CancellationToken ct = default);
    Task<ServiceResult<object>> DeleteAsync(int id, int actorUserId, CancellationToken ct = default);
}
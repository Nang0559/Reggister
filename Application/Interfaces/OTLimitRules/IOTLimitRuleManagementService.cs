using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.OTLimitRules;

public interface IOTLimitRuleManagementService
{
    Task<ServiceResult<List<OTLimitRuleDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<OTLimitRuleDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> CreateAsync(OTLimitRuleUpsertDto model, int currentUserId, CancellationToken ct = default);
    Task<ServiceResult> UpdateAsync(OTLimitRuleUpsertDto model, int currentUserId, CancellationToken ct = default);
    Task<ServiceResult> ToggleActiveAsync(int id, int currentUserId, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);

    Task<List<OTLimitRuleDto>> GetApplicableRulesAsync(
        string deptCode,
        string positionCode,
        OTLimitType limitType,
        CancellationToken ct = default);
}

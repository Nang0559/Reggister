using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.OTs;

public interface IOTLimitRuleManagementClientService
{
    Task<ApiResponse<List<OTLimitRuleDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<OTLimitRuleDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<object>> CreateAsync(OTLimitRuleUpsertDto model, CancellationToken ct = default);
    Task<ApiResponse<object>> UpdateAsync(int id, OTLimitRuleUpsertDto model, CancellationToken ct = default);
    Task<ApiResponse<object>> ToggleAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default);
}

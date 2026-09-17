using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Utils;


namespace FVN_REGISTER.Application.Interfaces.OTLimitRules
{
    public interface IOTLimitRuleManagementService
    {
        Task<ServiceResult<List<OTLimitRuleDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<OTLimitRuleDto>> GetByIdAsync(int id, CancellationToken ct = default);

        Task<ServiceResult> CreateAsync(OTLimitRuleUpsertDto model, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> UpdateAsync(OTLimitRuleUpsertDto model, int currentUserId, CancellationToken ct = default);
        Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Lấy rule áp dụng cho 1 nhân viên cụ thể (dùng khi validate đăng ký OT).
        /// Ưu tiên: Rule (Dept+Position) > Rule Dept-only > Rule Position-only > Rule toàn công ty.
        /// </summary>
        Task<List<OTLimitRuleDto>> GetApplicableRulesAsync(
            string deptCode, string positionCode, OTLimitType limitType, CancellationToken ct = default);
    }
}

using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Contract.Dtos.OtReasons;
using FVN_REGISTER.Contract.Dtos.OTTypeDtos;


namespace FVN_REGISTER.Contract.Dtos.OT
{
    /// <summary>
    /// Cấu hình OT dùng để render form đăng ký — tổng hợp từ OTType + Rules áp dụng
    /// cho đúng nhân viên (theo DeptCode/PositionCode của họ).
    /// </summary>
    public class OtConfigDto
    {
        public List<OTTypeDto> OtTypes { get; set; } = new();
        public List<OTReasonCodeDto> ReasonCodes { get; set; } = new();

        /// <summary>Các rule giới hạn áp dụng cho nhân viên này (đã lọc theo Dept/Position).</summary>
        public List<OTLimitRuleDto> ApplicableLimitRules { get; set; } = new();
    }
}

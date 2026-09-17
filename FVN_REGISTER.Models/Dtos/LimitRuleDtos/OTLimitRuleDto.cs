using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Contract.Dtos.LimitRuleDtos
{
    public class OTLimitRuleDto
    {
        public int Id { get; set; }
        public OTLimitType LimitType { get; set; }
        public decimal LimitValue { get; set; }
        public string? PositionCode { get; set; }
        public string? DeptCode { get; set; }
        public decimal LimitHours { get; set; }
        public string? Description { get; set; }

        // Hiển thị phạm vi áp dụng cho UI
        public string ScopeText =>
            (PositionCode, DeptCode) switch
            {
                (null, null) => "Toàn công ty",
                (not null, null) => $"Chức vụ: {PositionCode}",
                (null, not null) => $"Phòng ban: {DeptCode}",
                _ => $"Phòng {DeptCode} - Chức vụ {PositionCode}"
            };
    }
}

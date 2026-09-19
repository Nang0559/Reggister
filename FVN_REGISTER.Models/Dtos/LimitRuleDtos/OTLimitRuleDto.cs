using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Contract.Dtos.LimitRuleDtos
{
    public class OTLimitRuleDto
    {
        public int Id { get; set; }
        public OTLimitType LimitType { get; set; }
        public OTLimitScopeType ScopeType { get; set; }
        public string? ScopeCode { get; set; }
        public string? EmployeeCode { get; set; }
        public decimal LimitValue { get; set; }
        public string? PositionCode { get; set; }
        public string? DeptCode { get; set; }
        public decimal LimitHours { get; set; }
        public string? Description { get; set; }

        // Hiển thị phạm vi áp dụng cho UI
        public string ScopeText
        {
            get
            {
                if (ScopeType == OTLimitScopeType.Department)
                    return $"Tổng phòng ban: {ScopeCode ?? DeptCode}";
                if (ScopeType == OTLimitScopeType.Block)
                    return $"Tổng khối: {ScopeCode}";
                if (!string.IsNullOrWhiteSpace(EmployeeCode))
                    return $"Nhân viên: {EmployeeCode}";
                if (!string.IsNullOrWhiteSpace(DeptCode) && !string.IsNullOrWhiteSpace(PositionCode))
                    return $"Phòng {DeptCode} - Chức vụ {PositionCode}";
                if (!string.IsNullOrWhiteSpace(DeptCode))
                    return $"Phòng ban: {DeptCode}";
                if (!string.IsNullOrWhiteSpace(PositionCode))
                    return $"Chức vụ: {PositionCode}";
                return "Toàn công ty";
            }
        }\n    }
}

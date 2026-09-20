using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Extensions;


namespace FVN_REGISTER.Contract.Dtos.LimitRuleDtos
{
    public static class OTLimitRuleDisplayExtensions
    {
        /// <summary>Mô tả phạm vi áp dụng của rule để hiển thị trên bảng Admin.</summary>
        public static string ToTargetDescription(this OTLimitRuleDto dto)
        {
            return dto.ScopeType switch
            {
                FVN_REGISTER.Core.Enums.OTLimitScopeType.Department => $"Tổng phòng {dto.ScopeCode ?? dto.DeptCode}",
                FVN_REGISTER.Core.Enums.OTLimitScopeType.Block => $"Tổng khối {dto.ScopeCode}",
                _ when !string.IsNullOrWhiteSpace(dto.EmployeeCode) => $"Nhân viên {dto.EmployeeCode}",
                _ when !string.IsNullOrWhiteSpace(dto.DeptCode) && !string.IsNullOrWhiteSpace(dto.PositionCode)
                    => $"Phòng {dto.DeptCode} - Vị trí {dto.PositionCode}",
                _ when !string.IsNullOrWhiteSpace(dto.DeptCode) => $"Phòng {dto.DeptCode}",
                _ when !string.IsNullOrWhiteSpace(dto.PositionCode) => $"Vị trí {dto.PositionCode}",
                _ => "Toàn công ty"
            };
        }

        /// <summary>Điểm ưu tiên — càng nhỏ càng cụ thể/càng được ưu tiên khi có nhiều rule khớp.</summary>
        public static int ToPriorityScore(this OTLimitRuleDto dto)
        {
            return dto.ScopeType switch
            {
                FVN_REGISTER.Core.Enums.OTLimitScopeType.Employee when !string.IsNullOrWhiteSpace(dto.EmployeeCode) => 1,
                FVN_REGISTER.Core.Enums.OTLimitScopeType.Employee => 2,
                FVN_REGISTER.Core.Enums.OTLimitScopeType.Department => 1,
                FVN_REGISTER.Core.Enums.OTLimitScopeType.Block => 1,
                _ => 9
            };
        }

        /// <summary>Ủy quyền màu/style sang enum extension đã có sẵn.</summary>
        public static UIStyle GetUIStyle(this OTLimitRuleDto dto) => dto.LimitType.GetUIStyle();
    }
}

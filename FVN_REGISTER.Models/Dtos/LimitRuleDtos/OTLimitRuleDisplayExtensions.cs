using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Extensions;


namespace FVN_REGISTER.Contract.Dtos.LimitRuleDtos
{
    public static class OTLimitRuleDisplayExtensions
    {
        /// <summary>Mô tả phạm vi áp dụng của rule để hiển thị trên bảng Admin.</summary>
        public static string ToTargetDescription(this OTLimitRuleDto dto)
        {
            var hasDept = !string.IsNullOrWhiteSpace(dto.DeptCode);
            var hasPos = !string.IsNullOrWhiteSpace(dto.PositionCode);

            return (hasDept, hasPos) switch
            {
                (true, true) => $"Phòng {dto.DeptCode} - Vị trí {dto.PositionCode}",
                (true, false) => $"Phòng {dto.DeptCode}",
                (false, true) => $"Vị trí {dto.PositionCode}",
                _ => "Toàn công ty"
            };
        }

        /// <summary>Điểm ưu tiên — càng nhỏ càng cụ thể/càng được ưu tiên khi có nhiều rule khớp.</summary>
        public static int ToPriorityScore(this OTLimitRuleDto dto)
        {
            var hasDept = !string.IsNullOrWhiteSpace(dto.DeptCode);
            var hasPos = !string.IsNullOrWhiteSpace(dto.PositionCode);

            if (hasDept && hasPos) return 1;
            if (hasDept) return 2;
            if (hasPos) return 3;
            return 4;
        }

        /// <summary>Ủy quyền màu/style sang enum extension đã có sẵn.</summary>
        public static UIStyle GetUIStyle(this OTLimitRuleDto dto) => dto.LimitType.GetUIStyle();
    }
}

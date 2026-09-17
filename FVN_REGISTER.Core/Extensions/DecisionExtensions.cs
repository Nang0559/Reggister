using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Extensions
{
    public static class DecisionExtensions
    {
        

        // Lấy Text hiển thị (dùng cho Label, Tooltip)
        public static string GetDisplayName(this DecisionType decision) => decision switch
        {
            DecisionType.Approved => "Phê duyệt",
            DecisionType.Rejected => "Từ chối",
            DecisionType.Returned => "Trả về sửa",
            _ => "N/A"
        };
    }
}

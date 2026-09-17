using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Shared.Constants;

namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class DecisionUIExtensions
    {
        // Lấy màu sắc (dùng cho MudButton, MudChip)
        public static string GetColor(this DecisionType decision) => decision switch
        {
            DecisionType.Approved => UIConstants.ColorSuccess,
            DecisionType.Rejected => UIConstants.ColorError,
            DecisionType.Returned => UIConstants.ColorWarning,
            _ => UIConstants.ColorDefault
        };

        // Lấy Icon (dùng cho MudIcon)
        public static string GetIcon(this DecisionType decision) => decision switch
        {
            DecisionType.Approved => UIConstants.IconCheck,
            DecisionType.Rejected => UIConstants.IconClose,
            DecisionType.Returned => UIConstants.IconWarning, // Hoặc icon "Undo/RotateLeft"
            _ => string.Empty
        };

        
    }
}

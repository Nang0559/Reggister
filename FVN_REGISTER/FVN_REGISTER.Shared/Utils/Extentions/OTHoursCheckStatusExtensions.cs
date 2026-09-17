using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Shared.Constants;


namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class OTHourValidationStatusExtensions
    {
        public static string ToDisplayName(this OTHourValidationStatus status) => status switch
        {
            OTHourValidationStatus.Valid => "Hợp lệ",
            OTHourValidationStatus.Warning => "Cảnh báo",
           
            _ => "Không xác định"
        };

        public static UIStyle GetStatusStyle(this OTHourValidationStatus status) => status switch
        {
            OTHourValidationStatus.Valid => new UIStyle { Color = UIConstants.ColorSuccess },
            OTHourValidationStatus.Warning => new UIStyle { Color = UIConstants.ColorWarning },
         
            _ => new UIStyle { Color = UIConstants.ColorDefault }
        };
    }
}

using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Shared.Constants;


namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class OTReconciliationStatusUIExtensions
    {
        public static string ToColor(this OTReconciliationStatus status) => status switch
        {
            OTReconciliationStatus.Processed => UIConstants.ColorSuccess,
            OTReconciliationStatus.PendingConfirm => UIConstants.ColorWarning,
            OTReconciliationStatus.RequestNotApproved => UIConstants.ColorInfo,
            OTReconciliationStatus.NoRequest => UIConstants.ColorError,
            _ => UIConstants.ColorDefault
        };

        // Core/Extensions/OTHourValidationStatusExtensions.cs — bổ sung
        public static string ToColor(this OTHourValidationStatus status) => status switch
        {
            OTHourValidationStatus.Valid => UIConstants.ColorSuccess,
            OTHourValidationStatus.Warning => UIConstants.ColorWarning,
            _ => UIConstants.ColorDefault
        };
    }
}

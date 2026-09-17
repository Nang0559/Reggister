using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Shared.Constants;

namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class OtSyncSituationExtensions
    {
        public static OtSyncSituation ToOtSyncSituation(this string? raw) => raw switch
        {
            "DA_XU_LY" => OtSyncSituation.Processed,
            "CHUA_CONFIRM" => OtSyncSituation.PendingConfirm,
            "DON_CHUA_DUYET" => OtSyncSituation.PendingApproval,
            "CHUA_CO_DON" => OtSyncSituation.NoRequest,
            "CHUA_CHAMCONG" => OtSyncSituation.NoAttendance,
            _ => OtSyncSituation.Unknown
        };

        public static string ToColor(this OtSyncSituation situation) => situation switch
        {
            OtSyncSituation.Processed => UIConstants.ColorSuccess,
            OtSyncSituation.PendingConfirm => UIConstants.ColorWarning,
            OtSyncSituation.PendingApproval => UIConstants.ColorInfo,
            OtSyncSituation.NoRequest => UIConstants.ColorError,
            OtSyncSituation.NoAttendance => UIConstants.ColorSecondary,   // ← cần bổ sung
            _ => UIConstants.ColorDefault
        };
    }
}

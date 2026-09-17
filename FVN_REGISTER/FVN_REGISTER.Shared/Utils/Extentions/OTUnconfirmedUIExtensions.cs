using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Shared.Constants;


namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class OTUnconfirmedUIExtensions
    {
        public static string GetValidationColor(this OTUnconfirmedDto dto) => dto.ValidationStatus switch
        {
            OtHoursCheckStatus.Valid => UIConstants.ColorSuccess,
            OtHoursCheckStatus.Warning => UIConstants.ColorWarning,
            OtHoursCheckStatus.Exceeded => UIConstants.ColorError,
            _ => UIConstants.ColorDefault   // bắt luôn Unknown
        };

        public static string GetTinhHuongColor(this OTUnconfirmedDto dto) => dto.TinhHuong switch
        {
            OtSyncSituation.Processed => UIConstants.ColorSuccess,
            OtSyncSituation.PendingConfirm => UIConstants.ColorWarning,
            OtSyncSituation.PendingApproval => UIConstants.ColorInfo,
            OtSyncSituation.NoRequest => UIConstants.ColorError,
            OtSyncSituation.NoAttendance => UIConstants.ColorSecondary,
            _ => UIConstants.ColorDefault   // bắt luôn Unknown
        };
    }
}

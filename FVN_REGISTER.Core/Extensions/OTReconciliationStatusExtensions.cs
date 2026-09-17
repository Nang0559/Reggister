using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Core.Extensions
{
    public static class OTReconciliationStatusExtensions
    {
        public static string ToDisplayName(this OTReconciliationStatus status) => status switch
        {
            OTReconciliationStatus.NoRequest => "Chưa có đơn OT",
            OTReconciliationStatus.RequestNotApproved => "Đơn chưa được duyệt",
            OTReconciliationStatus.PendingConfirm => "Chờ xác nhận",
            OTReconciliationStatus.Processed => "Đã xử lý",
            _ => status.ToString()
        };

        public static string ToCode(this OTReconciliationStatus status) => status switch
        {
            OTReconciliationStatus.NoRequest => "CHUA_CO_DON",
            OTReconciliationStatus.RequestNotApproved => "DON_CHUA_DUYET",
            OTReconciliationStatus.PendingConfirm => "CHUA_CONFIRM",
            OTReconciliationStatus.Processed => "DA_XU_LY",
            _ => "UNKNOWN"
        };

        public static bool TryToOTReconciliationStatus(this string code, out OTReconciliationStatus status)
        {
            switch ((code ?? "").ToUpperInvariant())
            {
                case "CHUA_CO_DON": status = OTReconciliationStatus.NoRequest; return true;
                case "DON_CHUA_DUYET": status = OTReconciliationStatus.RequestNotApproved; return true;
                case "CHUA_CONFIRM": status = OTReconciliationStatus.PendingConfirm; return true;
                case "DA_XU_LY": status = OTReconciliationStatus.Processed; return true;
                default: status = default; return false;
            }
        }
    }

    public static class OTHourValidationStatusExtensions
    {
        public static string ToDisplayName(this OTHourValidationStatus status) => status switch
        {
            OTHourValidationStatus.Valid => "Hợp lệ",
            OTHourValidationStatus.Warning => "Cảnh báo",
            _ => status.ToString()
        };

        public static bool TryToOTHourValidationStatus(this string code, out OTHourValidationStatus status)
        {
            switch ((code ?? "").ToUpperInvariant())
            {
                case "VALID": status = OTHourValidationStatus.Valid; return true;
                case "WARNING": status = OTHourValidationStatus.Warning; return true;
                default: status = default; return false;
            }
        }
    }
}

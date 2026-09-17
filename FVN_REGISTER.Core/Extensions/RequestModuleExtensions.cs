using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Extensions
{
    public static class RequestModuleExtensions
    {
        public static string ToDisplayName(this RequestModule module) => module switch
        {
            RequestModule.Leave => "Nghỉ phép",
            RequestModule.Overtime => "Tăng ca",
            RequestModule.Trip => "Công tác",
            _ => module.ToString()
        };

        public static string ToCode(this RequestModule module) => module switch
        {
            RequestModule.Leave => "LEAVE",
            RequestModule.Overtime => "OT",
            RequestModule.Trip => "TRIP",
            _ => "UNKNOWN"
        };

        public static string ToUnitLabel(this RequestModule module) => module switch
        {
            RequestModule.Overtime => "giờ",
            _ => "ngày"
        };

        public static string ToDetailPath(this RequestModule module) => module switch
        {
            RequestModule.Leave => "/leaves/detail",
            RequestModule.Overtime => "/ot/detail",
            RequestModule.Trip => "/trips/detail",
            _ => "/dashboard"
        };
        public static bool TryToRequestModule(this string code, out RequestModule module)
        {
            switch (code.ToUpper())
            {
                case "LEAVE": module = RequestModule.Leave; return true;
                case "OT": module = RequestModule.Overtime; return true;
                case "TRIP": module = RequestModule.Trip; return true;
                default: module = default; return false;
            }
        }

    }
}

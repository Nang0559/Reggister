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

        public static RequestModule ParseCode(string? code) => code?.ToUpperInvariant() switch
        {
            "LEAVE" => RequestModule.Leave,
            "OT" => RequestModule.Overtime,
            "TRIP" => RequestModule.Trip,
            _ => RequestModule.Leave
        };
        public static RequestModule ToRequestModule(this string code) => ParseCode(code);
        public static bool TryToRequestModule(this string code, out RequestModule module)
        {
            module = ParseCode(code);
            return code != null && code.Equals(module.ToCode(), StringComparison.OrdinalIgnoreCase);
        }
    }
}

using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class RequestModuleUIExtensions
    {

        // ✅ Overload 1 — cho RequestModule, bắt buộc phải có hàm này
        public static string GetEventColor(this RequestModule module) => module switch
        {
            RequestModule.Leave => "#FF9800",
            RequestModule.Overtime => "#2196F3",
            _ => "#9E9E9E"
        };

        // ✅ Overload 2 — cho CalendarEventDto, gọi lại overload 1 ở trên
        public static string GetEventColor(this CalendarEventDto e) => e.Module.GetEventColor();
        public static UIStyle GetStatusStyle(this RequestModule module) => module switch
        {
            RequestModule.Leave => new UIStyle
            {
                Icon = IconConstants.Module.Leave,
                Color = "info",
                CssClass = "badge-leave"
            },
            RequestModule.Overtime => new UIStyle
            {
                Icon = IconConstants.Module.Overtime,
                Color = "warning",
                CssClass = "badge-ot"
            },
            RequestModule.Trip => new UIStyle
            {
                Icon = IconConstants.Module.Trip,
                Color = "primary",
                CssClass = "badge-trip"
            },
            _ => new UIStyle
            {
                Icon = IconConstants.Module.Default,
                Color = "default",
                CssClass = "badge-default"
            }
        };
    }
}

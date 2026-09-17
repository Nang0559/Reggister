using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Shared.Constants;
using FVN_REGISTER.Shared.Utils.Extensions;


namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class EnumUIExtensions
    {
        public static List<StatusOptionDto> ToStatusOptions<T>() where T : struct, Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e =>
                {
                    UIStyle ui = e switch
                    {
                        ApprovalStatus status => status.GetStatusStyle(),
                        EmailStatus emailStatus => emailStatus.GetStatusStyle(),
                        RequestModule module => module.GetStatusStyle(),   
                        _ => new UIStyle { Color = UIConstants.ColorDefault }
                    };

                    string displayName = e switch
                    {
                        ApprovalStatus status => status.ToDisplayName(),
                        RequestModule module => module.ToDisplayName(),
                        OTLimitType limitType => limitType.ToDisplayName(),
                        HalfDayType halfDay => halfDay.ToDisplayName(),
                        EmailStatus emailStatus => emailStatus.ToDisplayName(),
                        _ => e.ToString()
                    };

                    return new StatusOptionDto
                    {
                        Code = e.ToString(),
                        DisplayName = displayName,
                        ColorClass = ui.Color
                    };
                }).ToList();
        }
    }
}

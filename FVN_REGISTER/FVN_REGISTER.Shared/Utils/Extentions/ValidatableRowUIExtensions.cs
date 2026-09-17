

using FVN_REGISTER.Core.Interfaces;
using FVN_REGISTER.Core.Extensions;

namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class ValidatableRowUIExtensions
    {
        // Trả về tên màu MudBlazor, không phải CSS class
        public static string GetRowColor(this IValidatableRow row)
            => row.Status.ToColorName();
    }
}

using FVN_REGISTER.Core.Enums;
using System.Drawing;


namespace FVN_REGISTER.Core.Extensions
{
    public static class RowStatusExtensions
    {
        // Lấy màu sắc chuẩn MudBlazor (để dùng cho MudButton, MudChip, v.v.)
        public static string ToColorName(this RowStatus status) => status switch
        {
            RowStatus.Success => "Success",
            RowStatus.Error => "Error",
            RowStatus.Warning => "Warning",
            RowStatus.Info => "Info",
            _ => "Default"
        };

        // Lấy mô tả trạng thái (Dùng cho Tooltip hoặc Label)
        public static string ToDescription(this RowStatus status) => status switch
        {
            RowStatus.Success => "Hợp lệ",
            RowStatus.Error => "Có lỗi xảy ra",
            RowStatus.Warning => "Cần chú ý",
            RowStatus.Info => "Thông tin",
            _ => "Trạng thái mặc định"
        };
    }
}

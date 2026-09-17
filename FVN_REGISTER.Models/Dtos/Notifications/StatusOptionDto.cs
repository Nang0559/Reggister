using System;


namespace FVN_REGISTER.Contract.Dtos.Notifications
{
    public class StatusOptionDto
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        // Dùng cho các component hỗ trợ màu CSS như MudBlazor (success, warning, error, info)
        public string ColorClass { get; set; } = "default";

        // Dùng cho trường hợp cần màu hex tùy chỉnh
        public string? ColorCode { get; set; }
    }
}

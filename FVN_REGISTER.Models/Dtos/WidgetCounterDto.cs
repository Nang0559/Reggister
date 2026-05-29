using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos
{
    public class WidgetCounterDto
    {
        public string Title { get; set; } = string.Empty; // Ví dụ: "Chờ duyệt", "Hiện diện"
        public string Value { get; set; } = "0";          // Giá trị hiển thị
        public string Icon { get; set; } = string.Empty;  // MudBlazor Icon (e.g., Icons.Material.Filled.HourglassEmpty)
        public string Color { get; set; } = "primary";    // Màu sắc (Success, Error, Info...)
        public string? Link { get; set; }                 // Bấm vào thì nhảy đi đâu (e.g., "/leaves/pending")
    }
}

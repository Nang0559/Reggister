

using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.MasterData
{
    public class CalendarEventDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // Loại sự kiện để phân biệt Leave, OT, v.v.
        public RequestModule Module { get; set; }

        // Trạng thái chung
        public ApprovalStatus Status { get; set; }

        // Dữ liệu mở rộng tùy theo loại sự kiện
        public Dictionary<string, object> MetaData { get; set; } = new();
       
    }
}

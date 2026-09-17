using FVN_REGISTER.Core.Enums;



// Core/Entities/Common/F03Attachment.cs
namespace FVN_REGISTER.Core.Entities.Common
{
    public class F03Attachment
    {
        public int FileId { get; set; }
        public RequestModule Module { get; set; }      // phân biệt Leave/Overtime/Trip
        public int RequestId { get; set; }               // FK logic tới F03LeaveDay.Id / F03OTRequest.Id / F03StagingTrip.Id

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileExtension { get; set; }
        public long FileSize { get; set; }

        public bool? IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

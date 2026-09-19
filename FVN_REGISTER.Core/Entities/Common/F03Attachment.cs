using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;



// Core/Entities/Common/F03Attachment.cs
namespace FVN_REGISTER.Core.Entities.Common
{
    public class F03Attachment : BaseAuditEntity
    {
        [NotMapped]
        public int FileId { get => Id; set => Id = value; }
        public RequestModule Module { get; set; }      // phân biệt Leave/Overtime/Trip
        public int RequestId { get; set; }               // FK logic tới F03LeaveDay.Id / F03OTRequest.Id / F03StagingTrip.Id

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileExtension { get; set; }
        public long FileSize { get; set; }

    }
}

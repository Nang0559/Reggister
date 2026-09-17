using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.OT
{
    [Table("F03OTEmployees")]
  
    public partial class F03OTEmployee : BaseAuditEntity
    {
        [ForeignKey(nameof(OTRequest))]
        public int OTRequestId { get; set; }

        [Required, StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EmployeeName { get; set; }

        [StringLength(20)]
        public string? DeptCode { get; set; }

        [StringLength(100)]
        public string? DeptName { get; set; }

        [StringLength(10)]
        public string? CvCode { get; set; }

        [StringLength(20)]
        public string? OTTypeCode { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal OTHours { get; set; } // Đã sửa tên: OtHours -> OTHours

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ActualHours { get; set; }

        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }

        [StringLength(10)]
        public string? OTReasonCategoryCode { get; set; } // Đã sửa: OtReason -> OTReason

        [StringLength(500)]
        public string? OTReasonDetail { get; set; } // Đã sửa: OtReason -> OTReason

        [Column(TypeName = "decimal(3,1)")]
        public decimal OTRateMultiplier { get; set; } = 1.5m; // Đã sửa: OtRate -> OTRate

        // Dùng Enum để quản lý chặt chẽ hơn string
        public ValidationStatus ValidationStatus { get; set; } = ValidationStatus.Pending;

        [StringLength(255)]
        public string? ValidationMessage { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation
        public virtual F03OTRequest OTRequest { get; set; } = null!;
    }
}

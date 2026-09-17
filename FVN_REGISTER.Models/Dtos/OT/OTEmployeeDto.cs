


using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Core.Interfaces;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTEmployeeDto 
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DeptCode { get; set; }
        public string? DeptName { get; set; }
        public string? CvCode { get; set; }

        public string? OTTypeCode { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal OTHours { get; set; }
        public decimal OTRateMultiplier { get; set; } = 1.5m;

        public string OTReasonCategoryCode { get; set; } = string.Empty;
        public string? OTReasonCategoryName { get; set; }
        public string? OTReasonDetail { get; set; }
        public string? Note { get; set; }

        public ValidationStatus ValidationStatus { get; set; }
        public string? ValidationMessage { get; set; }
        public bool HasError => ValidationStatus == ValidationStatus.Invalid;

        // MỚI: màu hiển thị UI, suy ra từ ValidationStatus, không lưu DB
        public RowStatus DisplayStatus => ValidationStatus.ToRowStatus();
    }
}

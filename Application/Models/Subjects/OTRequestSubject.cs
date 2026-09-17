


using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Models.Subjects
{
    public sealed class OTRequestSubject : IApprovalSubject
    {
        public int RequestId { get; set; }
        public RequestModule Module => RequestModule.Overtime;
        public string EmployeeCode { get; set; } = "";
        public string? EmployeeName { get; set; }        // ← THÊM
        public string? DeptCode { get; set; }
        public string? PositionCode { get; set; }
        public ApprovalStatus OverallStatus { get; set; }
        public string OTCode { get; set; } = "";
        public DateTime OTDate { get; set; }
        public decimal TotalOTHours { get; set; }
        public string? OTReasonSummary { get; set; }
        public string OTTypeCode { get; set; } = "";

        // Đã xóa "int? OverrideLevel => null;" — đây là dead code do thiếu 'public',
        // không implement gì cả. IApprovalSubject.OverrideLevel đã có default = null sẵn,
        // chỉ cần override nếu OT thật sự có nhu cầu ghi đè level (ví dụ Admin duyệt tắt).

        public static OTRequestSubject From(F03OTRequest x, string? employeeName = null, string? PositionCode = null) => new()
        {
            RequestId = x.Id,
            EmployeeCode = x.EmployeeCode,
            EmployeeName = employeeName,
            DeptCode = x.DeptCode,
            PositionCode = PositionCode,
            OverallStatus = x.RequestStatus,

            OTCode = x.OTCode,
            OTDate = x.OTDate,
            TotalOTHours = x.TotalOTHours,
            OTReasonSummary = x.OTReasonSummary,
            OTTypeCode = x.OTTypeCode
        };
    }
}

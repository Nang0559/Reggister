using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    /// <summary>
    /// Kết quả 1 lần đối chiếu (reconcile) giờ OT thực tế với đơn đã đăng ký.
    /// Đây là Command-with-Result: usp_SyncOTActualHours vừa UPDATE F03OTEmployee.ActualHours
    /// vừa trả về báo cáo — KHÔNG phải query thuần, gọi lại nhiều lần sẽ ghi đè dữ liệu mỗi lần.
    /// </summary>
    public class OTReconciliationResultDto
    {
        public DateTime WorkDate { get; set; }
        public string? DeptCode { get; set; }
        public DateTime ReconciledAt { get; set; }

        public int TotalRows { get; set; }
        public int ProcessedCount { get; set; }        // TinhHuong = Processed
        public int PendingConfirmCount { get; set; }    // TinhHuong = PendingConfirm
        public int PendingApprovalCount { get; set; }    // TinhHuong = RequestNotApproved
        public int NoRequestCount { get; set; }          // TinhHuong = NoRequest
        public int ValidCount { get; set; }               // ValidationStatus = Valid
        public int WarningCount { get; set; }              // ValidationStatus = Warning

        public List<OTReconciliationItemDto> Items { get; set; } = new();

        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        public string Summary =>
            $"Đối chiếu {TotalRows} bản ghi. Đã xử lý: {ProcessedCount}, " +
            $"Chờ xác nhận: {PendingConfirmCount}, Chưa có đơn: {NoRequestCount}, Cảnh báo: {WarningCount}";
    }

    public class OTReconciliationItemDto
    {
        public DateTime WorkDate { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;

        public string? CheckInText { get; set; }
        public string? CheckOutText { get; set; }

        public decimal OTHoursActual { get; set; }
        public decimal OTHoursPlanned { get; set; }
        public decimal OTHoursRequest { get; set; }

        public bool IsHoliday { get; set; }
        public string? HolidayType { get; set; }
        public string? ShiftType { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftName { get; set; }

        public int? OTRequestId { get; set; }
        public string? OTCode { get; set; }
        public string? RequestStatus { get; set; }

        public OTHourValidationStatus ValidationStatus { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public OTReconciliationStatus Status { get; set; }

        public string StatusDisplay => Status.ToDisplayName();
    }
}

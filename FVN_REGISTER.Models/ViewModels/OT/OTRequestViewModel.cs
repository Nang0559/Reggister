using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class OTRequestViewModel
    {
        public int Id { get; set; }
        public string OTCode { get; set; } = string.Empty;

        // Người tạo đơn (bổ sung — OTQueryService.MapToViewModel cần)
        public string EmployeeCode { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }

        public string DeptCode { get; set; } = string.Empty;
        public string? DeptName { get; set; }

        public DateTime OTDate { get; set; }

        // OTType → OTTypeCode (khớp với CreateOTRequestModel và OTTypeConst)
        public string OTTypeCode { get; set; } = string.Empty;
        // OTTypeText → OTTypeName (naming convention nhất quán với LeaveTypeName)
        public string OTTypeName { get; set; } = string.Empty;

        // string → TimeSpan (khớp với F03OTRequest, tính được TotalOTHours)
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // PlannedHours → TotalOTHours (khớp với OTQueryService.MapToViewModel)
        public decimal TotalOTHours { get; set; }

        // OTReason → Reason (khớp với CreateOTRequestModel)
        public string? Reason { get; set; }

        // Phạm vi áp dụng (bổ sung — flow ảnh bước 1)
        public string ScopeType { get; set; } = "SELECTED";

        public string RequestStatus { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;

        public int EmployeeCount { get; set; }
        public List<OTApprovalStep> ApprovalSteps { get; set; } = new();
        public List<OTEmployeeModel> Employees { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Computed — giữ nguyên
        public bool CanEdit => OTStatus.ActiveStatuses.Contains(RequestStatus);
        public bool CanCancel => OTStatus.ActiveStatuses.Contains(RequestStatus);

        // Thêm computed tiện ích
        public string StartTimeText => StartTime.ToString(@"hh\:mm");
        public string EndTimeText => EndTime.ToString(@"hh\:mm");
    }
}

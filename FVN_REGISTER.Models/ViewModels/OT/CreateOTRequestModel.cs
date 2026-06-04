using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class CreateOTRequestModel
    {
        // --- Người tạo ---
        public string EmployeeCode { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
        public string CvCode { get; set; } = string.Empty;   // để check HasWorker

        // --- Thông tin OT ---
        [Required(ErrorMessage = "Chọn ngày OT")]
        public DateTime OTDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Chọn loại OT")]
        public string OTTypeCode { get; set; } = OTTypeConst.Weekday;  // khớp với OTTypeConst

        [Required(ErrorMessage = "Nhập giờ bắt đầu")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Nhập giờ kết thúc")]
        public TimeSpan EndTime { get; set; }

        // Tính tự động từ StartTime/EndTime, không cần nhập tay
        public decimal PlannedHours => EndTime > StartTime
            ? (decimal)(EndTime - StartTime).TotalHours
            : 0;

        [Required(ErrorMessage = "Nhập lý do OT")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;   // đổi từ OTReason → Reason

        // --- Phạm vi áp dụng ---
        public string ScopeType { get; set; } = "SELECTED";  // SELECTED | DEPARTMENT

        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 nhân viên")]
        public List<OTEmployeeModel> Employees { get; set; } = new();

        // --- Approvers (khớp đúng với flow: Bước 3, 5, 6, 7) ---
        // Bước 3: Sub-leader / Leader
        public string? Level3ApproveEmail { get; set; }
        public string? Level3ApproveCode { get; set; }
        public string? Level3ApproveName { get; set; }

        // Bước 5: Ast. Chief / Chief
        public string? Level5ApproveEmail { get; set; }
        public string? Level5ApproveCode { get; set; }
        public string? Level5ApproveName { get; set; }

        // Bước 6: A.MG / MG
        public string? Level6ApproveEmail { get; set; }
        public string? Level6ApproveCode { get; set; }
        public string? Level6ApproveName { get; set; }

        // Bước 7: GM (ngày thường 3 bộ đưa / ngày nhất định)
        public string? Level7ApproveEmail { get; set; }
        public string? Level7ApproveCode { get; set; }
        public string? Level7ApproveName { get; set; }

        // --- Dữ liệu phụ trợ cho UI (load từ GetCombinedDataAsync) ---
        public List<OTApprovalStep> ApprovalSteps { get; set; } = new();
        public List<F03OTLimitRule> LimitRules { get; set; } = new();
        public OTBalanceDto? Balance { get; set; }

        // --- Helpers ---
        // Công nhân (CVCode 0003) cần đủ 4 bước duyệt
        public bool HasWorker => Employees.Any(e => e.CvCode == "0003");

        // Nhân viên văn phòng chỉ cần từ Bước 5 trở lên
        public bool RequiresAllLevels => HasWorker;
    }
}

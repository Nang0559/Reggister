using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels.OT
{
    public class CreateOTRequestModel
    {
        [Required(ErrorMessage = "Chọn bộ phận")]
        public string DeptCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chọn ngày OT")]
        public DateTime OTDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Chọn loại OT")]
        public string OTType { get; set; } = "WEEKDAY";

        [Required(ErrorMessage = "Nhập giờ bắt đầu")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Nhập giờ kết thúc")]
        public TimeSpan EndTime { get; set; }

        [Range(0.5, 12, ErrorMessage = "Giờ OT phải từ 0.5 đến 12")]
        public decimal PlannedHours { get; set; }

        [Required(ErrorMessage = "Nhập lý do OT")]
        [StringLength(500)]
        public string OTReason { get; set; } = string.Empty;

        public string ScopeType { get; set; } = "SELECTED";

        // Danh sách nhân viên được chọn
        public List<OTEmployeeModel> Employees { get; set; } = new();

        // Approvers - tùy theo CVCode và tầng
        public string? Level1ApproveEmail { get; set; }
        public string? Level1ApproveCode { get; set; }
        public string? Level1ApproveName { get; set; }

        public string? Level2ApproveEmail { get; set; }
        public string? Level2ApproveCode { get; set; }
        public string? Level2ApproveName { get; set; }

        public string? Level3ApproveEmail { get; set; }
        public string? Level3ApproveCode { get; set; }
        public string? Level3ApproveName { get; set; }

        public string? Level4ApproveEmail { get; set; }
        public string? Level4ApproveCode { get; set; }
        public string? Level4ApproveName { get; set; }

        // Helper: có công nhân (CVCode 0003) trong danh sách không
        public bool HasWorker => Employees.Any(e => e.CvCode == "0003");
    }
}

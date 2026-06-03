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
        public int WorkYear { get; set; } = DateTime.Now.Year;

        // Người tạo đơn
        public string? EmployeeCode { get; set; }
        public string? DeptCode { get; set; }
        public string? CvCode { get; set; }
        public string? CreatedByEmail { get; set; }
        public string? CreatedByLevel { get; set; }

        // Thời gian OT
        public DateTime OTDate { get; set; } = DateTime.Today;
        public DateTime PlannedFrom { get; set; }
        public DateTime PlannedTo { get; set; }
        public decimal PlannedHours { get; set; }  // Tự tính, làm tròn 15 phút

        // Loại ngày: Normal | Weekend | Holiday | Tet
        public string DayType { get; set; } = "Normal";

        // Lý do
        public string? OTReason { get; set; }

        // Cờ bắt buộc GM (tự tính phía client và server đều validate lại)
        public bool RequiresGM { get; set; }
        public bool Lv3Skip { get; set; }  // NV văn phòng bỏ qua bước 3

        // ========== 4 CẤP DUYỆT ==========

        // Bước 3: Sub.Leader / Leader
        public string? Lv3ApproveCode { get; set; }
        public string? Lv3ApproveName { get; set; }
        public string? Lv3ApproveEmail { get; set; }

        // Bước 4: BCH Công đoàn
        public string? Lv4ApproveCode { get; set; }
        public string? Lv4ApproveName { get; set; }
        public string? Lv4ApproveEmail { get; set; }

        // Bước 5: Ast.Chief / Chief
        public string? Lv5ApproveCode { get; set; }
        public string? Lv5ApproveName { get; set; }
        public string? Lv5ApproveEmail { get; set; }

        // Bước 6: A.MG / MG
        public string? Lv6ApproveCode { get; set; }
        public string? Lv6ApproveName { get; set; }
        public string? Lv6ApproveEmail { get; set; }

        // Bước 7: GM (chỉ khi RequiresGM = true)
        public string? Lv7ApproveCode { get; set; }
        public string? Lv7ApproveName { get; set; }
        public string? Lv7ApproveEmail { get; set; }

        // Trạng thái: "Pending" | "Draft"
        public string RequestStatus { get; set; } = "Pending";


        public string ErrorMessage { get; set; } = string.Empty;
        public bool HasError { get; set; }

        // Danh sách nhân viên trong ca OT
        public List<OTEmployeeModel> Employees { get; set; } = new();
    }
}

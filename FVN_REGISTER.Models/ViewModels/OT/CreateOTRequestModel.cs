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
        public string? EmployeeCode { get; set; }
        public string? DeptCode { get; set; }
        public string? CvCode { get; set; }

        // Thời gian
        public DateTime OTDate { get; set; } = DateTime.Today;
        public DateTime PlannedFrom { get; set; }
        public DateTime PlannedTo { get; set; }
        public decimal PlannedHours { get; set; }   // Tự tính và validate

        // Loại ngày - tự tính dựa vào OTDate + bảng CompanyHoliday
        public string DayType { get; set; } = "Normal";

        public string OTReason { get; set; } = string.Empty;

        // ===== 4 CẤP KÝ =====
        // Bước 3: Sub.Leader / Leader
        public string? Lv3ApproveCode { get; set; }
        public string? Lv3ApproveName { get; set; }
        public string? Lv3ApproveEmail { get; set; }
        public bool Lv3Skip { get; set; }           // true nếu NV văn phòng

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

        // Cờ computed - tự tính trong Validator/Service
        public bool RequiresGM { get; set; }
        public string? CreatedByLevel { get; set; }     // Role của người tạo

        // Danh sách nhân viên OT (có thể nhiều người)
        public List<OTEmployeeModel> Employees { get; set; } = new();

        // WorkYear tự tính
        public int WorkYear => OTDate.Year;
    }
}

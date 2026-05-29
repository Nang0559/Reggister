using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels
{
    public class ApproverViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Chọn bộ phận")]
        [Display(Name = "Bộ phận (*)")]
        public string DeptCode { get; set; }

        [Required(ErrorMessage = "Chọn mã nhân viên")]
        [Display(Name = "Mã Nhân Viên (*)")]
        public string? ApproveLevelCode { get; set; }

        [Required(ErrorMessage = "Nhập Level Approve")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Chỉ cho phép nhập số.")]
        [Display(Name = "Level Approve (*)")]
        public int ApproveLevel { get; set; }

        [Required(ErrorMessage = "Nhập Họ Tên")]
        [MaxLength(200, ErrorMessage = "Độ dài không quá 200 Ký tự")]
        [Display(Name = "Tên Người Approve (*)")]
        public string? ApproveLevelName { get; set; }

        [Required(ErrorMessage = "Nhập Email")]
        [MaxLength(200, ErrorMessage = "Độ dài không quá 200 Ký tự")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email (*)")]
        public string? ApproveLevelEmail { get; set; }

        [Display(Name = "Kích hoạt (*)")]
        public bool IsActive { get; set; } = true;

        // Audit fields
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}

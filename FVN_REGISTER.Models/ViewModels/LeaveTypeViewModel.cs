
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.ViewModels
{
    public class LeaveTypeViewModel
    {
        public int LeaveTypeId { get; set; }

        [Required(ErrorMessage = "Mã")]
        [MaxLength(50, ErrorMessage = "Độ dài không quá 50 Ký tự")]
        [Display(Name = "Mã (*)")]
        public string LeaveTypeCode { get; set; }

        [Required(ErrorMessage = "Nhập tên")]
        [MaxLength(200, ErrorMessage = "Độ dài không quá 200 Ký tự")]
        [Display(Name = "Tên (*)")]
        public string LeaveTypeName { get; set; }


        [Display(Name = "Tính phép (*)")]
        public bool? TinhPhep { get; set; }


        [Display(Name = "Kích hoạt (*)")]
        public bool IsActive { get; set; }

        public LeaveTypeViewModel()
        {

        }
    }
}

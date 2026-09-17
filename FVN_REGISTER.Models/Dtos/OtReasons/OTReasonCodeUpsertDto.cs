using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OtReasons
{
    public class OTReasonCodeUpsertDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nhập mã lý do")]
        [MaxLength(10, ErrorMessage = "Độ dài không quá 10 ký tự")]
        [Display(Name = "Mã lý do (*)")]
        public string ReasonCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập tên hiển thị")]
        [MaxLength(100, ErrorMessage = "Độ dài không quá 100 ký tự")]
        [Display(Name = "Tên hiển thị (*)")]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Độ dài không quá 255 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Kích hoạt (*)")]
        public bool IsActive { get; set; } = true;
    }
}


using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Dtos.OTTypeDtos
{
    public class OTTypeUpsertDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nhập mã loại tăng ca")]
        [MaxLength(50, ErrorMessage = "Độ dài không quá 50 ký tự")]
        [Display(Name = "Mã loại tăng ca (*)")]
        public string OTTypeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập tên loại tăng ca")]
        [MaxLength(200, ErrorMessage = "Độ dài không quá 200 ký tự")]
        [Display(Name = "Tên loại tăng ca (*)")]
        public string OTTypeName { get; set; } = string.Empty;

        [MaxLength(200)]
        [Display(Name = "Tên phụ")]
        public string? OTTypeName2 { get; set; }

        [Required(ErrorMessage = "Nhập hệ số lương")]
        [Range(0.1, 10, ErrorMessage = "Hệ số phải từ 0.1 đến 10")]
        [Display(Name = "Hệ số lương (*)")]
        public decimal RateMultiplier { get; set; } = 1.5m;

        [Display(Name = "Kích hoạt (*)")]
        public bool IsActive { get; set; } = true;
    }
}

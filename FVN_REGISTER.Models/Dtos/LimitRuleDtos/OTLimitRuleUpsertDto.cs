using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Dtos.LimitRuleDtos
{
    public class OTLimitRuleUpsertDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Chọn loại giới hạn")]
        [Display(Name = "Loại giới hạn (*)")]
        public OTLimitType LimitType { get; set; }

        public OTLimitScopeType ScopeType { get; set; } = OTLimitScopeType.Employee;

        [MaxLength(50)]
        public string? ScopeCode { get; set; }

        [MaxLength(50)]
        public string? EmployeeCode { get; set; }

        [Range(0, 999, ErrorMessage = "Giá trị không hợp lệ")]
        [Display(Name = "Giá trị giới hạn")]
        public decimal LimitValue { get; set; }

        [MaxLength(20)]
        [Display(Name = "Chức vụ áp dụng")]
        public string? PositionCode { get; set; }

        [MaxLength(20)]
        [Display(Name = "Phòng ban áp dụng")]
        public string? DeptCode { get; set; }

        [Required(ErrorMessage = "Nhập số giờ giới hạn")]
        [Range(0.1, 999, ErrorMessage = "Số giờ phải lớn hơn 0")]
        [Display(Name = "Số giờ giới hạn (*)")]
        public decimal LimitHours { get; set; }

        [MaxLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
    }
}

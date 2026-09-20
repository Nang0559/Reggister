using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Dtos.LimitRuleDtos;

public class OTLimitRuleUpsertDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Chọn loại giới hạn")]
    public OTLimitType LimitType { get; set; }

    [Required]
    public OTLimitScopeType ScopeType { get; set; } = OTLimitScopeType.Employee;

    [MaxLength(50)]
    public string? ScopeCode { get; set; }

    [MaxLength(50)]
    public string? EmployeeCode { get; set; }

    [Range(0.1, 999, ErrorMessage = "Số giờ phải lớn hơn 0")]
    public decimal LimitHours { get; set; }

    // Trường tương thích dữ liệu cũ; backend luôn chuẩn hóa theo LimitHours.
    public decimal LimitValue { get; set; }

    [MaxLength(20)]
    public string? PositionCode { get; set; }

    [MaxLength(20)]
    public string? DeptCode { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Users;

public sealed class PasswordResetRequestCreateDto
{
    [Required(ErrorMessage = "Mã nhân viên không được để trống")]
    [StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? RequestNote { get; set; }
}

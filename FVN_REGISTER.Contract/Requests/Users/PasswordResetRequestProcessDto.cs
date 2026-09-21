using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Users;

public sealed class PasswordResetRequestProcessDto
{
    public bool Approve { get; set; } = true;

    [StringLength(1000)]
    public string? ResultNote { get; set; }
}

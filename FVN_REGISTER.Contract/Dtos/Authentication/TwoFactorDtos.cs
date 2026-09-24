namespace FVN_REGISTER.Contract.Dtos.Authentication;

public sealed class TwoFactorStatusDto
{
    public bool Required { get; set; }
    public bool Enabled { get; set; }
}

public sealed class TwoFactorSetupDto
{
    public bool Required { get; set; }
    public string Secret { get; set; } = string.Empty;
    public string OtpAuthUri { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string QrCodeDataUri { get; set; } = string.Empty;
}


public sealed class TwoFactorAdminUserDto
{
    public int UserId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? DeptCode { get; set; }
    public bool IsActive { get; set; }
    public bool Required { get; set; }
    public bool Enabled { get; set; }
    public DateTime? RequiredAt { get; set; }
    public DateTime? EnabledAt { get; set; }
}

public sealed class TwoFactorRequirementRequest { public bool Required { get; set; } }

namespace FVN_REGISTER.Contract.Dtos.EmailTemplates;

public sealed class EmailProfileDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EmailServerName { get; set; } = string.Empty;
    public string EmailServerType { get; set; } = "SMTP";
    public int EmailServerPort { get; set; } = 587;
    public bool EmailServerEnableSsl { get; set; } = true;
    public string SecurityMode { get; set; } = "STARTTLS";
    public string AuthenticationType { get; set; } = "Basic";
    public string EmailAccountName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string? ReplyTo { get; set; }
    public string? SiteUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public bool HasPassword { get; set; }
    public string? Password { get; set; }
}

public sealed class EmailTestRequest
{
    public string ToEmail { get; set; } = string.Empty;
}

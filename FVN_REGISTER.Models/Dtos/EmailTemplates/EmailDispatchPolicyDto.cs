namespace FVN_REGISTER.Contract.Dtos.EmailTemplates;

public sealed class EmailDispatchPolicyDto
{
    public int Id { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string EmailProfileCode { get; set; } = string.Empty;
    public string DispatchMode { get; set; } = "AutoSend";
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}

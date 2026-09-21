namespace FVN_REGISTER.Contract.Dtos.PublicInformation;
public sealed class PublicInformationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = "Announcement";
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsImportant { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
}
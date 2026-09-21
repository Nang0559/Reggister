using System.ComponentModel.DataAnnotations;
namespace FVN_REGISTER.Contract.Requests.PublicInformation;
public sealed class SavePublicInformationRequest
{
    [Required, StringLength(30)] public string Type { get; set; } = "Announcement";
    [Required, StringLength(300)] public string Title { get; set; } = string.Empty;
    [StringLength(1000)] public string? Summary { get; set; }
    public string? Content { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsImportant { get; set; }
    [StringLength(1000)] public string? AttachmentUrl { get; set; }
}
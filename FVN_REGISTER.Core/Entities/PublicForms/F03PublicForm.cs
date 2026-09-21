using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities;

namespace FVN_REGISTER.Core.Entities.PublicForms;

[Table("F03PublicForms")]
public sealed class F03PublicForm : BaseAuditEntity
{
    [Required, StringLength(50)] public string FormCode { get; set; } = string.Empty;
    [Required, StringLength(300)] public string Title { get; set; } = string.Empty;
    [StringLength(2000)] public string? Description { get; set; }
    [StringLength(50)] public string? CategoryCode { get; set; }
    [Required, StringLength(20)] public string Status { get; set; } = "Draft";
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public bool AllowMultipleSubmit { get; set; }
    public bool RequireApproval { get; set; }
    public int? MaxSubmissions { get; set; }
    public int Version { get; set; } = 1;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public ICollection<F03PublicFormQuestion> Questions { get; set; } = new List<F03PublicFormQuestion>();
    public ICollection<F03PublicFormAudience> Audiences { get; set; } = new List<F03PublicFormAudience>();
}
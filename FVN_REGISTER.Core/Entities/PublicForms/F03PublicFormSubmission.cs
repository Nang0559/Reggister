using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities.Common;

namespace FVN_REGISTER.Core.Entities.PublicForms;

[Table("F03PublicFormSubmissions")]
public sealed class F03PublicFormSubmission : BaseAuditEntity
{
    public int FormId { get; set; }
    [Required, StringLength(50)] public string EmployeeCode { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    [Required, StringLength(20)] public string Status { get; set; } = "Submitted";
    public int FormVersion { get; set; } = 1;
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledBy { get; set; }
    public ICollection<F03PublicFormAnswer> Answers { get; set; } = new List<F03PublicFormAnswer>();
}
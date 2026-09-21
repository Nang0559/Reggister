using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities.Common;

namespace FVN_REGISTER.Core.Entities.PublicForms;

[Table("F03PublicFormQuestions")]
public sealed class F03PublicFormQuestion : BaseAuditEntity
{
    public int FormId { get; set; }
    [Required, StringLength(50)] public string QuestionCode { get; set; } = string.Empty;
    [Required, StringLength(1000)] public string QuestionText { get; set; } = string.Empty;
    [Required, StringLength(30)] public string QuestionType { get; set; } = "Text";
    [StringLength(1000)] public string? HelpText { get; set; }
    [StringLength(300)] public string? Placeholder { get; set; }
    public bool IsRequired { get; set; }
    public int Sequence { get; set; } = 1;
    public ICollection<F03PublicFormQuestionOption> Options { get; set; } = new List<F03PublicFormQuestionOption>();
}
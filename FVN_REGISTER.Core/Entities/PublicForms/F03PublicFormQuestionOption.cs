using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities.Common;

namespace FVN_REGISTER.Core.Entities.PublicForms;

[Table("F03PublicFormQuestionOptions")]
public sealed class F03PublicFormQuestionOption : BaseAuditEntity
{
    public int QuestionId { get; set; }
    [Required, StringLength(50)] public string OptionCode { get; set; } = string.Empty;
    [Required, StringLength(300)] public string OptionText { get; set; } = string.Empty;
    public int Sequence { get; set; } = 1;
}
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities;

namespace FVN_REGISTER.Core.Entities.PublicForms;

[Table("F03PublicFormAnswers")]
public sealed class F03PublicFormAnswer : BaseAuditEntity
{
    public int SubmissionId { get; set; }
    public int QuestionId { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumberValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BoolValue { get; set; }
    public string? JsonValue { get; set; }
}
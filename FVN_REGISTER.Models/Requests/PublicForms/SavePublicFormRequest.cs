using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.PublicForms;

public sealed class SavePublicFormRequest
{
    [Required, StringLength(50)] public string FormCode { get; set; } = string.Empty;
    [Required, StringLength(300)] public string Title { get; set; } = string.Empty;
    [StringLength(2000)] public string? Description { get; set; }
    [StringLength(50)] public string? CategoryCode { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public bool AllowMultipleSubmit { get; set; }
    public bool RequireApproval { get; set; }
    [Range(1, int.MaxValue)] public int? MaxSubmissions { get; set; }
    public List<SavePublicFormQuestionRequest> Questions { get; set; } = new();
    public List<SavePublicFormAudienceRequest> Audiences { get; set; } = new();
}
public sealed class SavePublicFormQuestionRequest
{
    [Required, StringLength(50)] public string QuestionCode { get; set; } = string.Empty;
    [Required, StringLength(1000)] public string QuestionText { get; set; } = string.Empty;
    [Required, StringLength(30)] public string QuestionType { get; set; } = "Text";
    public string? HelpText { get; set; }
    public string? Placeholder { get; set; }
    public bool IsRequired { get; set; }
    public int Sequence { get; set; } = 1;
    public List<SavePublicFormOptionRequest> Options { get; set; } = new();
}
public sealed class SavePublicFormOptionRequest
{
    [Required, StringLength(50)] public string OptionCode { get; set; } = string.Empty;
    [Required, StringLength(300)] public string OptionText { get; set; } = string.Empty;
    public int Sequence { get; set; } = 1;
}
public sealed class SavePublicFormAudienceRequest
{
    [Required, StringLength(20)] public string ScopeType { get; set; } = "AllCompany";
    [StringLength(100)] public string? ScopeValue { get; set; }
}

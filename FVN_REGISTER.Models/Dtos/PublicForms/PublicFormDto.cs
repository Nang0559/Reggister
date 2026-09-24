namespace FVN_REGISTER.Contract.Dtos.PublicForms;

public sealed class PublicFormDto
{
    public int Id { get; set; }
    public string FormCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CategoryCode { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public bool AllowMultipleSubmit { get; set; }
    public bool RequireApproval { get; set; }
    public int? MaxSubmissions { get; set; }
    public int Version { get; set; }
    public bool? IsActive { get; set; }
    public List<PublicFormQuestionDto> Questions { get; set; } = new();
    public List<PublicFormAudienceDto> Audiences { get; set; } = new();
}
public sealed class PublicFormQuestionDto
{
    public int Id { get; set; }
    public string QuestionCode { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "Text";
    public string? HelpText { get; set; }
    public string? Placeholder { get; set; }
    public bool IsRequired { get; set; }
    public int Sequence { get; set; }
    public List<PublicFormOptionDto> Options { get; set; } = new();
}
public sealed class PublicFormOptionDto
{
    public int Id { get; set; }
    public string OptionCode { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public int Sequence { get; set; }
}
public sealed class PublicFormAudienceDto
{
    public int Id { get; set; }
    public string ScopeType { get; set; } = "AllCompany";
    public string? ScopeValue { get; set; }
}

public sealed class PublicFormAudienceLookupDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Secondary { get; set; }
}

public sealed class PublicFormAudienceEmployeePageDto
{
    public List<PublicFormAudienceLookupDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

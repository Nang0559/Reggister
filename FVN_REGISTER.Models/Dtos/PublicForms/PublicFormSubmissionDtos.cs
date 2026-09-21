namespace FVN_REGISTER.Contract.Dtos.PublicForms;

public sealed class PublicFormSubmissionQueryDto
{
    public string? DepartmentCode { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class PublicFormSubmissionListDto
{
    public int FormId { get; set; }
    public string FormCode { get; set; } = string.Empty;
    public string FormTitle { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<PublicFormQuestionDto> Questions { get; set; } = new();
    public List<PublicFormSubmissionRowDto> Items { get; set; } = new();
}

public sealed class PublicFormSubmissionRowDto
{
    public int SubmissionId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DeptCode { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PublicFormSubmissionAnswerDto> Answers { get; set; } = new();
}

public sealed class PublicFormSubmissionAnswerDto
{
    public int QuestionId { get; set; }
    public string QuestionCode { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class PublicFormSubmissionSummaryDto
{
    public int FormId { get; set; }
    public int TotalSubmissions { get; set; }
    public List<PublicFormDepartmentSummaryDto> ByDepartment { get; set; } = new();
    public List<PublicFormQuestionSummaryDto> ByQuestion { get; set; } = new();
}

public sealed class PublicFormDepartmentSummaryDto
{
    public string DeptCode { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class PublicFormQuestionSummaryDto
{
    public int QuestionId { get; set; }
    public string QuestionCode { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public List<PublicFormChoiceSummaryDto> Choices { get; set; } = new();
}

public sealed class PublicFormChoiceSummaryDto
{
    public string OptionCode { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public int Count { get; set; }
}

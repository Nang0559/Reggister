namespace FVN_REGISTER.Contract.Requests.PublicForms;
public sealed class PublicFormAnswerRequest
{
    public int QuestionId { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumberValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BoolValue { get; set; }
    public string? JsonValue { get; set; }
}
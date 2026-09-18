namespace FVN_REGISTER.Contract.Dtos.HrmSync;

public sealed class HrmSyncRunResultDto
{
    public Guid RunId { get; set; }
    public bool Success { get; set; }
    public bool Manual { get; set; }
    public bool Running { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? TriggeredBy { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<HrmSyncJobRunDto> Jobs { get; set; } = new();
}

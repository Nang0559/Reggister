namespace FVN_REGISTER.Contract.Dtos.HrmSync;

public sealed class HrmSyncRuntimeStatusDto
{
    public bool IsRunning { get; set; }
    public Guid? CurrentRunId { get; set; }
    public DateTime? StartedAt { get; set; }
    public string? TriggeredBy { get; set; }
    public HrmSyncRunResultDto? LastRun { get; set; }
}

using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class OTWorkerStatusDto
    {
        public WorkerRunState State { get; set; } = WorkerRunState.Waiting;       // WAITING | RUNNING | ERROR | STOPPED
        public string StateText { get; set; } = string.Empty;
        public DateTime? NextRunAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public DateTime? LastRunFinishedAt { get; set; }
        public string? LastRunResult { get; set; }           // "OK" | "FAILED" | null
        public string? LastRunMessage { get; set; }
        public int? LastSyncedCount { get; set; }
        public string ServerTime { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }
}

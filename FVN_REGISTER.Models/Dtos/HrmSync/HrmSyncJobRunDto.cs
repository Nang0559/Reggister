namespace FVN_REGISTER.Contract.Dtos.HrmSync;

public sealed class HrmSyncJobRunDto
{
    public string EntityType { get; set; } = string.Empty;
    public int SyncOrder { get; set; }
    public bool IsBlockingDependency { get; set; }
    public bool Success { get; set; }
    public int TotalSource { get; set; }
    public int Added { get; set; }
    public int Updated { get; set; }
    public int Deactivated { get; set; }
    public int Unchanged { get; set; }
    public int Superseded { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

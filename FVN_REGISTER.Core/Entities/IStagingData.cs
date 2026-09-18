namespace FVN_REGISTER.Core.Entities;

public interface IStagingData
{
    int Id { get; }
    bool IsProcessed { get; set; }
    string? ErrorMessage { get; set; }
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
}

namespace FVN_REGISTER.Contract.Dtos.Actions;

public sealed class ActionItemDto
{
    public Guid ActionId { get; init; }
    public string ModuleCode { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public int EmployeeId { get; init; }
    public int AssignedToEmployeeId { get; init; }
    public DateOnly? WorkDate { get; init; }
    public string ActionType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public byte Severity { get; init; }
    public int Priority { get; init; }
    public byte Status { get; init; }
    public DateTime? DueAt { get; init; }
    public string? DetailRoute { get; init; }
    public string? ReferenceNo { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

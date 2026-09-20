namespace FVN_REGISTER.Application.Models.Actions;

public sealed record ActionItemDraft(
    string ModuleCode,
    string SourceId,
    int EmployeeId,
    int AssignedToEmployeeId,
    int? AssignedToUserId,
    DateOnly? WorkDate,
    string ActionType,
    string Title,
    string? Summary,
    byte Severity,
    int Priority,
    DateTime? DueAt,
    string? DetailRoute,
    string? ReferenceNo,
    string? PayloadJson);

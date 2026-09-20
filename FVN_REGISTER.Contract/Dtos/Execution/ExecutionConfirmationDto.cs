namespace FVN_REGISTER.Contract.Dtos.Execution;

public sealed record ExecutionConfirmationDto(
    long Id,
    long ReconciliationId,
    string ModuleCode,
    string SourceType,
    string SourceId,
    string? ParticipantId,
    int EmployeeId,
    DateOnly WorkDate,
    string? Decision,
    string Status,
    string? Comment,
    bool EvidenceRequired,
    DateTime? SubmittedAt,
    DateTime? ReviewedAt,
    string? ReviewNote);
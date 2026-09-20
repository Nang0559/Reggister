namespace FVN_REGISTER.Contract.Dtos.Execution;

public sealed record ExecutionConfirmationDto(
    long Id,
    long ReconciliationId,
    string ModuleCode,
    string SourceId,
    int EmployeeId,
    DateOnly WorkDate,
    string? Decision,
    string Status,
    string? Comment,
    bool EvidenceRequired,
    DateTime? SubmittedAt,
    DateTime? ReviewedAt,
    string? ReviewNote);
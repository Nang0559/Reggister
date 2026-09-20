namespace FVN_REGISTER.Contract.Dtos.Execution;

public sealed record ExecutionReconciliationDto(
    long Id,
    string ModuleCode,
    string SourceType,
    string SourceId,
    string? ParticipantId,
    int EmployeeId,
    DateOnly WorkDate,
    string? PlannedState,
    string? ActualState,
    string ReconciliationStatus,
    bool RequiresConfirmation,
    bool RequiresEvidence,
    long? ConfirmationId,
    Guid? ActionId);
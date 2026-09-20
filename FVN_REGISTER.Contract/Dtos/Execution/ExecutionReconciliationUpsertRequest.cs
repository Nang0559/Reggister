namespace FVN_REGISTER.Contract.Dtos.Execution;

public sealed record ExecutionReconciliationUpsertRequest(
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
    string? DetailJson);
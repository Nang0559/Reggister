namespace FVN_REGISTER.Contract.Dtos.Execution;

public sealed record ExecutionHrResolutionRequest(
    string Decision,
    string Reason,
    string CalendarAction = "KEEP");

public sealed record ExecutionHrReviewItemDto(
    long Id,
    string ModuleCode,
    string SourceType,
    string SourceId,
    string? ParticipantId,
    int EmployeeId,
    string EmployeeCode,
    string? EmployeeName,
    DateOnly WorkDate,
    string? PlannedState,
    string? ActualState,
    string ReconciliationStatus,
    bool RequiresConfirmation,
    bool RequiresEvidence,
    long? ConfirmationId,
    Guid? ActionId,
    DateTime? ResolvedAt);

public sealed record ExecutionHrResolutionDto(
    long ResolutionId,
    long ReconciliationId,
    string Decision,
    string Reason,
    string CalendarAction,
    DateTime ResolvedAt);
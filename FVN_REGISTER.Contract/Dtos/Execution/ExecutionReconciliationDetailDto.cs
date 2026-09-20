namespace FVN_REGISTER.Contract.Dtos.Execution;

public sealed record ExecutionHrResolutionSummaryDto(
    long Id,
    string Decision,
    string Reason,
    string CalendarAction,
    DateTime ResolvedAt);

public sealed record ExecutionReconciliationDetailDto(
    ExecutionReconciliationDto Reconciliation,
    ExecutionConfirmationDto? Confirmation,
    IReadOnlyList<ExecutionEvidenceDto> Evidence,
    ExecutionHrResolutionSummaryDto? HrResolution);

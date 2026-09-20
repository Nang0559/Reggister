namespace FVN_REGISTER.Contract.Dtos.Execution;

public sealed record ExecutionEvidenceReviewRequest(
    string ReviewStatus,
    string? ReviewNote);

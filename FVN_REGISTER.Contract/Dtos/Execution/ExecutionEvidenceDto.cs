namespace FVN_REGISTER.Contract.Dtos.Execution;

public sealed record ExecutionEvidenceDto(
    long Id,
    long ConfirmationId,
    string EvidenceType,
    int? FileId,
    string? ReferenceNo,
    string? ExternalUrl,
    string? Description,
    string ReviewStatus,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    string? ReviewNote);
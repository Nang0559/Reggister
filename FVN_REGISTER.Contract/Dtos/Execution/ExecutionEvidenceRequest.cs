namespace FVN_REGISTER.Contract.Dtos.Execution;

public sealed record ExecutionEvidenceRequest(
    string EvidenceType,
    int? FileId,
    string? ReferenceNo,
    string? ExternalUrl,
    string? Description);
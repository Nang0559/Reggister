namespace FVN_REGISTER.Contract.Dtos.Execution;

public sealed record ExecutionConfirmationRequest(
    string Decision,
    string? Comment,
    bool EvidenceRequired);
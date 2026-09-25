namespace FVN_REGISTER.Contract.Dtos.Security;

public sealed record WebSecurityFunctionCandidateDto(
    string FunctionKey,
    string Module,
    string Action,
    string Source,
    string SourceType,
    string? Route,
    string? Component,
    string DisplayName,
    string Description);

public sealed record WebSecurityManifestRequest(
    IReadOnlyList<WebSecurityFunctionCandidateDto> Entries);

public sealed record WebSecurityManifestSummaryDto(
    DateTime ReceivedAt,
    int Received,
    int NewCandidates,
    int UpdatedCandidates,
    int Registered,
    int Ignored);

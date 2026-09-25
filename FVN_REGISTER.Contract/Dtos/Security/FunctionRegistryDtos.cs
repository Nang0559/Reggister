namespace FVN_REGISTER.Contract.Dtos.Security;

public sealed record SecurityFunctionRegistryItemDto(
    int Id,
    string FunctionKey,
    int FunctionCode,
    string DefinitionName,
    string? ModuleCode,
    string? ActionCode,
    string? ScopeCode,
    string LifecycleStatus,
    string SourceType,
    string? SourceAssembly,
    string? SourceTypeName,
    string? ReplacementFunctionKey,
    DateTime FirstDiscoveredAt,
    DateTime LastSeenAt,
    DateTime? ResolvedAt,
    bool IsIgnored,
    bool Registered);

public sealed record SecurityFunctionDiscoverySummaryDto(
    DateTime ScannedAt,
    int Discovered,
    int Matched,
    int PendingRegistration,
    int PendingRetirement,
    int Conflict);

public sealed record RegisterDiscoveredFunctionRequest(
    string? FunctionName = null,
    string? Detail = null,
    string? ModuleCode = null,
    string? ActionCode = null,
    string? ScopeCode = null);

public sealed record ReplaceFunctionRequest(string ReplacementFunctionKey);

public sealed record SecurityFunctionUpsertRequest(
    int FunctionCode,
    string FunctionKey,
    string FunctionName,
    string Detail,
    string? ModuleCode,
    string? ActionCode,
    string? ScopeCode,
    int DisplayOrder = 0);

public sealed record SecurityRoleUpsertRequest(
    int RoleCode,
    string RoleName,
    string? Detail,
    bool IsSystem = false);

public sealed record SecurityRoleDto(
    int Id,
    int RoleCode,
    string RoleName,
    string? Detail,
    bool IsSystem,
    bool IsActive);

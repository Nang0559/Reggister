namespace FVN_REGISTER.Core.Constants;

public static class AuthorizationScopePolicy
{
    public static string ResolveEffectiveScope(IEnumerable<string?> scopes)
    {
        var normalized = scopes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Any(x => string.Equals(x, AuthorizationScopeCodes.All, StringComparison.OrdinalIgnoreCase)))
            return AuthorizationScopeCodes.All;
        if (normalized.Any(x => string.Equals(x, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase)))
            return AuthorizationScopeCodes.Department;
        if (normalized.Any(x => string.Equals(x, AuthorizationScopeCodes.Employee, StringComparison.OrdinalIgnoreCase)))
            return AuthorizationScopeCodes.Employee;
        if (normalized.Any(x => string.Equals(x, AuthorizationScopeCodes.Own, StringComparison.OrdinalIgnoreCase)))
            return AuthorizationScopeCodes.Own;

        return AuthorizationScopeCodes.None;
    }

    public static bool CanAccess(
        string scope,
        string? actorEmployeeCode,
        string? actorDeptCode,
        string? targetEmployeeCode,
        string? targetDeptCode)
    {
        return scope switch
        {
            var s when string.Equals(s, AuthorizationScopeCodes.All, StringComparison.OrdinalIgnoreCase) => true,
            var s when string.Equals(s, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase) =>
                Same(actorDeptCode, targetDeptCode),
            var s when string.Equals(s, AuthorizationScopeCodes.Own, StringComparison.OrdinalIgnoreCase)
                || string.Equals(s, AuthorizationScopeCodes.Employee, StringComparison.OrdinalIgnoreCase) =>
                Same(actorEmployeeCode, targetEmployeeCode),
            _ => false
        };
    }

    private static bool Same(string? left, string? right) =>
        !string.IsNullOrWhiteSpace(left)
        && !string.IsNullOrWhiteSpace(right)
        && string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}

using FVN_REGISTER.Core.Constants;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class AuthorizationScopePolicyTests
{
    [Fact]
    public void ResolveEffectiveScope_UsesMostPermissiveConfiguredScope()
    {
        Assert.Equal(
            AuthorizationScopeCodes.All,
            AuthorizationScopePolicy.ResolveEffectiveScope(new[]
            {
                AuthorizationScopeCodes.Own,
                AuthorizationScopeCodes.Department,
                AuthorizationScopeCodes.All
            }));
    }

    [Fact]
    public void CanAccess_DepartmentScope_OnlyMatchesDepartment()
    {
        Assert.True(AuthorizationScopePolicy.CanAccess(
            AuthorizationScopeCodes.Department,
            "HR01",
            "D01",
            "EMP99",
            "D01"));

        Assert.False(AuthorizationScopePolicy.CanAccess(
            AuthorizationScopeCodes.Department,
            "HR01",
            "D01",
            "EMP99",
            "D02"));
    }

    [Fact]
    public void CanAccess_OwnAndEmployeeScopes_MatchEmployee()
    {
        Assert.True(AuthorizationScopePolicy.CanAccess(
            AuthorizationScopeCodes.Own,
            "EMP01",
            "D01",
            "EMP01",
            "D02"));

        Assert.True(AuthorizationScopePolicy.CanAccess(
            AuthorizationScopeCodes.Employee,
            "EMP01",
            "D01",
            "EMP01",
            "D02"));

        Assert.False(AuthorizationScopePolicy.CanAccess(
            AuthorizationScopeCodes.Own,
            "EMP01",
            "D01",
            "EMP02",
            "D01"));
    }

    [Fact]
    public void CanAccess_NoneScope_DeniesAccess()
    {
        Assert.False(AuthorizationScopePolicy.CanAccess(
            AuthorizationScopeCodes.None,
            "EMP01",
            "D01",
            "EMP01",
            "D01"));
    }
}

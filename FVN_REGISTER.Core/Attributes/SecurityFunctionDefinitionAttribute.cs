namespace FVN_REGISTER.Core.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SecurityFunctionDefinitionAttribute : Attribute
{
    public SecurityFunctionDefinitionAttribute(string functionKey, string? displayName = null)
    {
        FunctionKey = functionKey;
        DisplayName = displayName;
    }

    public string FunctionKey { get; }
    public string? DisplayName { get; }
    public string? ModuleCode { get; init; }
    public string? ActionCode { get; init; }
    public string? ScopeCode { get; init; }
}

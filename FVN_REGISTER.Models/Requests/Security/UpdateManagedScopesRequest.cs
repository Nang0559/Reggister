using FVN_REGISTER.Contract.Dtos.Security;

namespace FVN_REGISTER.Contract.Requests.Security;

public sealed class UpdateManagedScopesRequest
{
    public int UserId { get; set; }
    public List<ManagedScopeRequest> Scopes { get; set; } = new();
}

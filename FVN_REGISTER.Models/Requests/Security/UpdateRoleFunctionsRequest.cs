namespace FVN_REGISTER.Contract.Requests.Security;

public sealed class UpdateRoleFunctionsRequest
{
    public int RoleCode { get; set; }
    public List<int> FunctionCodes { get; set; } = new();
}

namespace FVN_REGISTER.Contract.Requests.Security;

public sealed class UpdateUserRolesRequest
{
    public int UserId { get; set; }
    public List<int> RoleCodes { get; set; } = new();
}

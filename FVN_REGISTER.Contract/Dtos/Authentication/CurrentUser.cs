namespace FVN_REGISTER.Contract.Dtos.Authentication;

/// <summary>
/// Backward-compatible current-user projection used by legacy equipment inspection code.
/// New code should consume UserIdentityDto directly.
/// </summary>
public sealed class CurrentUser
{
    public int UserId { get; init; }
    public string? EmployeeCode { get; init; }
    public string? DeptCode { get; init; }
    public string? PositionCode { get; init; }

    public static implicit operator CurrentUser?(UserIdentityDto? user)
        => user == null
            ? null
            : new CurrentUser
            {
                UserId = user.UserId,
                EmployeeCode = user.EmployeeCode,
                DeptCode = user.DeptCode,
                PositionCode = user.PositionCode
            };
}

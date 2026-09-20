namespace FVN_REGISTER.Contract.Dtos.Actions;

public sealed class ActionCountDto
{
    public int OpenCount { get; init; }
    public int InProgressCount { get; init; }
    public int TotalOpenCount => OpenCount + InProgressCount;
}

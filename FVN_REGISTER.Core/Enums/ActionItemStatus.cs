namespace FVN_REGISTER.Core.Enums;

public enum ActionItemStatus : byte
{
    Open = 0,
    InProgress = 10,
    Completed = 20,
    Dismissed = 30,
    Expired = 40,
    Cancelled = 90
}

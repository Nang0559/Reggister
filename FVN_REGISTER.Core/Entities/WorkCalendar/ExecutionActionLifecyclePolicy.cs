using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;

/// <summary>
/// Central business transition rules for an execution-confirmation action.
/// The worker is responsible for persistence/timing; this class owns the state decision.
/// </summary>
public static class ExecutionActionLifecyclePolicy
{
    public static ActionItemStatus ResolveDueStatus(string? reconciliationStatus)
    {
        return reconciliationStatus?.Trim() switch
        {
            "Resolved" => ActionItemStatus.Completed,
            "Mismatch" => ActionItemStatus.InProgress,
            "AwaitingConfirmation" => ActionItemStatus.InProgress,
            "Matched" => ActionItemStatus.Cancelled,
            "None" => ActionItemStatus.Expired,
            _ => ActionItemStatus.Expired
        };
    }
}

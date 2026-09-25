namespace FVN_REGISTER.Core.Enums;

/// <summary>
/// Controls how an email queue item is dispatched after template/policy evaluation.
/// The numeric order is persisted by EF/SQL, so keep these values stable.
/// </summary>
public enum EmailDispatchMode
{
    /// <summary>Queue and allow the email worker to send automatically.</summary>
    AutoSend = 0,

    /// <summary>Queue the email but require an explicit approval before sending.</summary>
    QueueForApproval = 1,

    /// <summary>Do not dispatch the email.</summary>
    Disabled = 2
}

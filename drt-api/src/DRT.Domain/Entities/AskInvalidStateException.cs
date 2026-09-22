namespace DRT.Domain.Entities;

/// <summary>
/// Thrown when a Cancel (or other state-transition) is attempted on an ASK
/// that is already in a terminal or otherwise disallowed state.
/// Maps to HTTP 409 Conflict with error code concurrency_conflict.
/// </summary>
public sealed class AskInvalidStateException : Exception
{
    public int AskId { get; }
    public int CurrentStatusId { get; }

    public AskInvalidStateException(int askId, int currentStatusId, string message)
        : base(message)
    {
        AskId = askId;
        CurrentStatusId = currentStatusId;
    }
}

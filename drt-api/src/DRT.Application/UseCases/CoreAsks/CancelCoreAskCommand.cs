namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Command for cancelling an existing Core ASK.
/// </summary>
public sealed class CancelCoreAskCommand
{
    /// <summary>The ASK to cancel, taken from the route parameter.</summary>
    public int AskId { get; init; }

    /// <summary>Mandatory cancellation comment supplied by the caller.</summary>
    public string Comment { get; init; } = string.Empty;

    /// <summary>Authenticated user identity (derived from validated token, never from request body).</summary>
    public string CancelledBy { get; init; } = string.Empty;

    /// <summary>Internal actor ID resolved from the authenticated identity.</summary>
    public int ActorId { get; init; }
}

namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Request body for POST /core-asks/{askId}/cancel (US-ASK-015).
/// </summary>
public sealed class CancelCoreAskRequest
{
    /// <summary>
    /// Mandatory cancellation comment. Must be non-empty.
    /// </summary>
    public string Comment { get; init; } = string.Empty;

    /// <summary>
    /// Must match the askId path parameter.
    /// </summary>
    public int AskId { get; init; }

    /// <summary>
    /// System-populated from authenticated user context. Not trusted from request body.
    /// </summary>
    public string? CancelledBy { get; init; }
}

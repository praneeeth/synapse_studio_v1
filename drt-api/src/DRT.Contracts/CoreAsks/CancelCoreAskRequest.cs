namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Request contract for POST /core-asks/{askId}/cancel (US-ASK-015).
/// </summary>
public sealed class CancelCoreAskRequest
{
    /// <summary>Must match the path parameter askId.</summary>
    public int AskId { get; init; }

    /// <summary>Required, non-empty cancellation comment.</summary>
    public string? Comment { get; init; }

    /// <summary>System-populated from authenticated user context; not trusted from caller.</summary>
    public string? CancelledBy { get; init; }
}

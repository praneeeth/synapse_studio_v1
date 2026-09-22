namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Request body for POST /core-asks/{askId}/cancel.
/// cancelledBy is system-populated from the authenticated user context and must not be
/// accepted as authoritative from the request body (SEC 003).
/// </summary>
public sealed class CancelCoreAskRequest
{
    /// <summary>Mandatory cancellation comment.</summary>
    public string? Comment { get; init; }
}

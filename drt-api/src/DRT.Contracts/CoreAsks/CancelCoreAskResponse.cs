namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Response payload for POST /core-asks/{askId}/cancel (US-ASK-015).
/// Returned with HTTP 200 OK on successful cancellation.
/// </summary>
public sealed class CancelCoreAskResponse
{
    /// <summary>The unique identifier of the cancelled Core ASK.</summary>
    public int AskId { get; init; }

    /// <summary>Status ID after cancellation. Always 577 (Cancelled).</summary>
    public int StatusId { get; init; }

    /// <summary>UTC timestamp when the cancellation was applied.</summary>
    public DateTimeOffset CancelledAt { get; init; }

    /// <summary>Identity (UPN) of the user who cancelled the ASK.</summary>
    public string CancelledBy { get; init; } = string.Empty;

    /// <summary>Audit entry created for this cancellation.</summary>
    public CancelAuditEntry AuditEntry { get; init; } = new();
}

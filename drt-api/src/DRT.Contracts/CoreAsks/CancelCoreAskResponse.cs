namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Response contract for POST /core-asks/{askId}/cancel (US-ASK-015).
/// </summary>
public sealed class CancelCoreAskResponse
{
    public int AskId { get; init; }

    /// <summary>Status after cancellation: 577 (Cancelled).</summary>
    public int StatusId { get; init; }

    public DateTimeOffset CancelledAt { get; init; }
    public string? CancelledBy { get; init; }
    public CancelAuditEntry? AuditEntry { get; init; }
}

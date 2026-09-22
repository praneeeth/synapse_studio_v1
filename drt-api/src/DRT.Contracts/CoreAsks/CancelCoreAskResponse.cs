namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Response payload for a successful Core ASK cancellation.
/// </summary>
public sealed class CancelCoreAskResponse
{
    public int AskId { get; init; }

    /// <summary>Status ID after cancellation. Value: 577 (Cancelled).</summary>
    public int StatusId { get; init; }

    public DateTimeOffset CancelledAt { get; init; }

    public string CancelledBy { get; init; } = string.Empty;

    public CancelAuditEntry AuditEntry { get; init; } = new();
}

/// <summary>
/// Audit entry summary returned in the cancellation response.
/// </summary>
public sealed class CancelAuditEntry
{
    public string Action { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset CreatedOn { get; init; }
}

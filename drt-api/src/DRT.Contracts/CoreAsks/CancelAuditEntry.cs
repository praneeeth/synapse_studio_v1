namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Audit entry included in the Cancel Core ASK response (US-ASK-015).
/// </summary>
public sealed class CancelAuditEntry
{
    /// <summary>Always "Core Ask Cancelled".</summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>The cancellation comment value.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Identity (UPN) of the user who performed the cancellation.</summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the audit entry was created.</summary>
    public DateTimeOffset CreatedOn { get; init; }
}

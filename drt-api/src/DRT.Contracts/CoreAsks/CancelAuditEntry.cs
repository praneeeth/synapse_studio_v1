namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Audit entry summary returned in the Cancel Core ASK response.
/// </summary>
public sealed class CancelAuditEntry
{
    public string Action { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CreatedBy { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}

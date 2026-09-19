namespace DRT.Application.Abstractions;

/// <summary>
/// Represents the resolved DRT actor for the current request.
/// </summary>
public sealed class CurrentActor
{
    public int DrtUserId { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string EntraObjectId { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<int> BusinessUnits { get; init; } = Array.Empty<int>();

    // TODO: US-ASK-001 - Leadership group determination logic is not defined.
    // Exact role IDs, BU scope, or service method for leadership group membership must be confirmed (Open Item #2).
    public bool IsInLeadershipGroup { get; init; }
}

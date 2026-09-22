namespace DRT.Application.Abstractions;

/// <summary>
/// Represents the resolved DRT actor from the authenticated identity.
/// </summary>
public sealed class DrtActor
{
    public int ActorId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = [];
    public int? BuScopeId { get; init; }
    /// <summary>
    /// Whether the actor is in the leadership group for routing purposes.
    /// TODO: Confirm exact logic for leadership group determination (open item: Leadership Group Determination).
    /// </summary>
    public bool IsLeadership { get; init; }
}

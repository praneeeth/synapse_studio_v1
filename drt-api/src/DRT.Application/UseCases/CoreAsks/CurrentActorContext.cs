namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Carries the resolved actor into the use case without coupling to HTTP.
/// </summary>
public sealed class CurrentActorContext
{
    public int DrtUserId { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();
    // TODO: US-ASK-001 - Leadership group determination logic must be confirmed (Open Item #2).
    public bool IsInLeadershipGroup { get; init; }
}

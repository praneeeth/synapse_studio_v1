using DRT.Application.Abstractions;

namespace DRT.Application.Abstractions;

/// <summary>
/// Resolves the current DRT actor from the authenticated HTTP context.
/// </summary>
public interface IActorResolver
{
    Task<DrtActor?> ResolveAsync(CancellationToken cancellationToken = default);
}

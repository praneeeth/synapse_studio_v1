using DRT.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DRT.Infrastructure.Persistence;

/// <summary>
/// Resolves the current DRT actor from the authenticated HTTP context.
/// TODO: Confirm exact claim types and leadership group determination logic (open item: Leadership Group Determination).
/// </summary>
public sealed class ActorResolver : IActorResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ActorResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<DrtActor?> ResolveAsync(CancellationToken cancellationToken = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        // TODO: Confirm exact claim type for actor ID from Entra ID JWT (open item: Authorization Permission Code).
        var actorIdClaim = user.FindFirst("actorId") ?? user.FindFirst(ClaimTypes.NameIdentifier);
        if (actorIdClaim == null || !int.TryParse(actorIdClaim.Value, out var actorId))
            return null;

        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // TODO: Confirm exact logic for leadership group determination (open item: Leadership Group Determination).
        var isLeadership = false; // TODO: replace with confirmed leadership group check

        await Task.CompletedTask;

        return new DrtActor
        {
            ActorId = actorId,
            UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            Roles = roles,
            IsLeadership = isLeadership
        };
    }
}

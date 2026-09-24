using DRT.Contracts.CoreAsks;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Result returned by the SearchCoreAsks use case.
/// </summary>
public sealed class SearchCoreAsksResult
{
    public SearchCoreAsksResponse Response { get; init; } = new();
}

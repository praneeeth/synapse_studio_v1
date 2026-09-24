using DRT.Application.UseCases.CoreAsks;

namespace DRT.Application.Abstractions;

/// <summary>
/// Port for the Search Core ASKs use case (Story 1859).
/// </summary>
public interface ISearchCoreAsksUseCase
{
    Task<SearchCoreAsksResult> ExecuteAsync(
        SearchCoreAsksQuery query,
        CancellationToken cancellationToken = default);
}

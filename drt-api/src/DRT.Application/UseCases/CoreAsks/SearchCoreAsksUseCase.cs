using DRT.Application.Abstractions;
using DRT.Contracts.CoreAsks;
using Microsoft.Extensions.Logging;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Implements the Search Core ASKs use case (Story 1859).
/// Processing steps:
///   1. Receive validated query (validation done by SearchCoreAsksQueryValidator before invocation).
///   2. Delegate to ICoreAskSearchRepository to execute the bounded, projected, read-only query.
///   3. Return paginated response with optional totalCount.
/// This is a read-only operation; no transaction boundary is required (DATA 003).
/// </summary>
public sealed class SearchCoreAsksUseCase : ISearchCoreAsksUseCase
{
    private readonly ICoreAskSearchRepository _searchRepository;
    private readonly ILogger<SearchCoreAsksUseCase> _logger;

    public SearchCoreAsksUseCase(
        ICoreAskSearchRepository searchRepository,
        ILogger<SearchCoreAsksUseCase> logger)
    {
        _searchRepository = searchRepository;
        _logger = logger;
    }

    public async Task<SearchCoreAsksResult> ExecuteAsync(
        SearchCoreAsksQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Searching Core ASKs: Page={Page} PageSize={PageSize} IncludeTotalCount={IncludeTotalCount}",
            query.Page, query.PageSize, query.IncludeTotalCount);

        var items = await _searchRepository.SearchAsync(query, cancellationToken);

        int? totalCount = null;
        if (query.IncludeTotalCount)
        {
            totalCount = await _searchRepository.CountAsync(query, cancellationToken);
        }

        var response = new SearchCoreAsksResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return new SearchCoreAsksResult { Response = response };
    }
}

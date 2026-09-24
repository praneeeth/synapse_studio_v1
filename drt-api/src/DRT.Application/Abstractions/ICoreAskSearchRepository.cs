using DRT.Application.UseCases.CoreAsks;
using DRT.Contracts.CoreAsks;

namespace DRT.Application.Abstractions;

/// <summary>
/// Port for the read-only Core ASK search query (Story 1859).
/// Separated from ICoreAskRepository to keep the write-side port focused
/// and to allow independent projection optimisation on the read side.
/// </summary>
public interface ICoreAskSearchRepository
{
    /// <summary>
    /// Returns the paginated list of Core ASK search items (latest version per askId).
    /// Uses AsNoTracking and server-side projection.
    /// </summary>
    Task<IReadOnlyList<CoreAskSearchItem>> SearchAsync(
        SearchCoreAsksQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total count of matching Core ASK records (latest version per askId).
    /// Called only when IncludeTotalCount is true.
    /// </summary>
    Task<int> CountAsync(
        SearchCoreAsksQuery query,
        CancellationToken cancellationToken = default);
}

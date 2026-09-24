using DRT.Contracts.CoreAsks;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Read-only query for searching Core ASKs (Story 1859).
/// Carries validated, resolved parameters from the controller.
/// </summary>
public sealed class SearchCoreAsksQuery
{
    /// <summary>Sort criteria. Null or empty means default sort (askId desc).</summary>
    public IReadOnlyList<SortCriterion>? Sort { get; init; }

    /// <summary>1-based page number.</summary>
    public int Page { get; init; }

    /// <summary>Records per page (already capped by validator).</summary>
    public int PageSize { get; init; }

    /// <summary>Whether to compute and return TotalCount.</summary>
    public bool IncludeTotalCount { get; init; }

    /// <summary>Optional allowlisted filters.</summary>
    public SearchCoreAsksFilters? Filters { get; init; }
}

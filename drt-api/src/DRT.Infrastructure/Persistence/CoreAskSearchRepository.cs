using DRT.Application.Abstractions;
using DRT.Application.UseCases.CoreAsks;
using DRT.Contracts.CoreAsks;
using Microsoft.EntityFrameworkCore;

namespace DRT.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of ICoreAskSearchRepository (Story 1859).
/// Read-only, AsNoTracking, server-side projection.
/// BR-001: Returns only the latest version per askId.
/// BR-002: Default sort is askId descending.
/// BR-004: Pagination is bounded by the validated pageSize.
/// BR-005: Filters are applied only against allowlisted fields using parameterized LINQ.
/// BR-006: Active records only unless filters explicitly request inactive.
/// </summary>
public sealed class CoreAskSearchRepository : ICoreAskSearchRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CoreAskSearchRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CoreAskSearchItem>> SearchAsync(
        SearchCoreAsksQuery query,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = BuildBaseQuery(query);
        baseQuery = ApplySort(baseQuery, query);

        var offset = (query.Page - 1) * query.PageSize;

        var items = await baseQuery
            .Skip(offset)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<int> CountAsync(
        SearchCoreAsksQuery query,
        CancellationToken cancellationToken = default)
    {
        return await BuildBaseQuery(query).CountAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds the base projected query: latest version per askId, active records,
    /// optional filters applied.
    /// BR-001: Subquery selects MAX(version) per askId.
    /// BR-006: isActive filter defaults to true unless caller overrides.
    /// </summary>
    private IQueryable<CoreAskSearchItem> BuildBaseQuery(SearchCoreAsksQuery query)
    {
        // Determine effective isActive filter (BR-006: active by default)
        bool? isActiveFilter = query.Filters?.IsActive ?? true;

        // Latest version per askId using a subquery join (BR-001)
        // Joins Ask -> AskVersion (latest) -> CoreAskDetail
        // TODO: Confirm exact EF Core navigation property names once entity model is finalised (OI-006).
        var latestVersions = _dbContext.AskVersions
            .AsNoTracking()
            .GroupBy(v => v.AskId)
            .Select(g => new { AskId = g.Key, MaxVersion = g.Max(v => v.Version) });

        var baseQuery = _dbContext.Asks
            .AsNoTracking()
            .Join(
                latestVersions,
                a => a.AskId,
                lv => lv.AskId,
                (a, lv) => new { Ask = a, lv.MaxVersion })
            .Join(
                _dbContext.AskVersions.AsNoTracking(),
                x => new { x.Ask.AskId, Version = x.MaxVersion },
                v => new { v.AskId, v.Version },
                (x, v) => new { x.Ask, AskVersion = v })
            .Join(
                _dbContext.CoreAskDetails.AsNoTracking(),
                x => x.Ask.AskId,
                d => d.AskId,
                (x, d) => new { x.Ask, x.AskVersion, Detail = d });

        // BR-006: active filter
        if (isActiveFilter.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Detail.IsActive == isActiveFilter.Value);
        }

        // Apply allowlisted filters (BR-005)
        if (query.Filters != null)
        {
            var f = query.Filters;

            if (f.AskId != null && f.AskId.Count > 0)
                baseQuery = baseQuery.Where(x => f.AskId.Contains(x.Ask.AskId));

            if (f.AskDetailId != null && f.AskDetailId.Count > 0)
                baseQuery = baseQuery.Where(x => f.AskDetailId.Contains(x.Detail.CoreAskDetailId));

            if (f.Version.HasValue)
                baseQuery = baseQuery.Where(x => x.AskVersion.Version == f.Version.Value);

            if (f.IsCompleted.HasValue)
                baseQuery = baseQuery.Where(x => x.Detail.IsCompleted == f.IsCompleted.Value);

            if (f.FiscalYear != null && f.FiscalYear.Count > 0)
                baseQuery = baseQuery.Where(x => f.FiscalYear.Contains(x.Detail.FYear));

            if (f.DppGroupId != null && f.DppGroupId.Count > 0)
                baseQuery = baseQuery.Where(x => f.DppGroupId.Contains(x.Detail.DppGroupId));

            // String-based filters (allowlisted, parameterized via LINQ Contains)
            if (f.DppGroup != null && f.DppGroup.Count > 0)
                baseQuery = baseQuery.Where(x => f.DppGroup.Contains(x.Detail.DppGroupName));

            if (f.Level != null && f.Level.Count > 0)
                baseQuery = baseQuery.Where(x => f.Level.Contains(x.Detail.LevelNeedName));

            if (f.Speciality != null && f.Speciality.Count > 0)
                baseQuery = baseQuery.Where(x => f.Speciality.Contains(x.Detail.GeneralSpecialityNeedName));

            if (f.Reason != null && f.Reason.Count > 0)
                baseQuery = baseQuery.Where(x => f.Reason.Contains(x.Detail.NeedReasonName));

            if (f.Status != null && f.Status.Count > 0)
                baseQuery = baseQuery.Where(x => f.Status.Contains(x.Detail.StatusName));
        }

        // Project to response DTO (BR-007: nullable fields default to 'N/A')
        return baseQuery.Select(x => new CoreAskSearchItem
        {
            AskId = x.Ask.AskId,
            RequestNumberDisplay = x.Detail.RequestNumberDisplay ?? string.Empty,
            CoreAskName = x.Detail.CoreAskName,
            DppGroup = x.Detail.DppGroupName ?? string.Empty,
            LevelNeed = x.Detail.LevelNeedName ?? "N/A",
            OutGoingResource = x.Detail.OutgoingResource ?? "N/A",
            NeedReason = x.Detail.NeedReasonName ?? "N/A",
            FYear = x.Detail.FYear.ToString(),
            ProjectedStartDate = x.Detail.ProjectedStartDate != default
                ? x.Detail.ProjectedStartDate.ToString("yyyy-MM-dd")
                : "N/A",
            EndDate = x.Detail.EndDate.HasValue
                ? x.Detail.EndDate.Value.ToString("yyyy-MM-dd")
                : "N/A",
            GeneralSpecialityNeed = x.Detail.GeneralSpecialityNeedName ?? "N/A",
            RolePosting = x.Detail.RolePostingName ?? "N/A",
            Version = x.AskVersion.Version,
            IsCompleted = x.Detail.IsCompleted,
            StatusId = x.Ask.StatusId
        });
    }

    /// <summary>
    /// Applies sort criteria from the query.
    /// BR-002: Default sort is askId descending when no sort criteria are provided.
    /// </summary>
    private static IQueryable<CoreAskSearchItem> ApplySort(
        IQueryable<CoreAskSearchItem> query,
        SearchCoreAsksQuery searchQuery)
    {
        if (searchQuery.Sort == null || searchQuery.Sort.Count == 0)
        {
            // BR-002: default sort
            return query.OrderByDescending(x => x.AskId);
        }

        IOrderedQueryable<CoreAskSearchItem>? ordered = null;

        foreach (var criterion in searchQuery.Sort)
        {
            bool isDesc = string.Equals(criterion.Direction, "desc", StringComparison.OrdinalIgnoreCase);

            if (ordered == null)
            {
                ordered = criterion.Field.ToLowerInvariant() switch
                {
                    "askid" => isDesc ? query.OrderByDescending(x => x.AskId) : query.OrderBy(x => x.AskId),
                    "requestnumberdisplay" => isDesc ? query.OrderByDescending(x => x.RequestNumberDisplay) : query.OrderBy(x => x.RequestNumberDisplay),
                    "coreaskname" => isDesc ? query.OrderByDescending(x => x.CoreAskName) : query.OrderBy(x => x.CoreAskName),
                    "dppgroup" => isDesc ? query.OrderByDescending(x => x.DppGroup) : query.OrderBy(x => x.DppGroup),
                    "levelneed" => isDesc ? query.OrderByDescending(x => x.LevelNeed) : query.OrderBy(x => x.LevelNeed),
                    "outgoingresource" => isDesc ? query.OrderByDescending(x => x.OutGoingResource) : query.OrderBy(x => x.OutGoingResource),
                    "needreason" => isDesc ? query.OrderByDescending(x => x.NeedReason) : query.OrderBy(x => x.NeedReason),
                    "fyear" => isDesc ? query.OrderByDescending(x => x.FYear) : query.OrderBy(x => x.FYear),
                    "projectedstartdate" => isDesc ? query.OrderByDescending(x => x.ProjectedStartDate) : query.OrderBy(x => x.ProjectedStartDate),
                    "enddate" => isDesc ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
                    "generalspecialityneed" => isDesc ? query.OrderByDescending(x => x.GeneralSpecialityNeed) : query.OrderBy(x => x.GeneralSpecialityNeed),
                    "roleposting" => isDesc ? query.OrderByDescending(x => x.RolePosting) : query.OrderBy(x => x.RolePosting),
                    "version" => isDesc ? query.OrderByDescending(x => x.Version) : query.OrderBy(x => x.Version),
                    "iscompleted" => isDesc ? query.OrderByDescending(x => x.IsCompleted) : query.OrderBy(x => x.IsCompleted),
                    "statusid" => isDesc ? query.OrderByDescending(x => x.StatusId) : query.OrderBy(x => x.StatusId),
                    _ => query.OrderByDescending(x => x.AskId) // fallback to default
                };
            }
            else
            {
                ordered = criterion.Field.ToLowerInvariant() switch
                {
                    "askid" => isDesc ? ordered.ThenByDescending(x => x.AskId) : ordered.ThenBy(x => x.AskId),
                    "requestnumberdisplay" => isDesc ? ordered.ThenByDescending(x => x.RequestNumberDisplay) : ordered.ThenBy(x => x.RequestNumberDisplay),
                    "coreaskname" => isDesc ? ordered.ThenByDescending(x => x.CoreAskName) : ordered.ThenBy(x => x.CoreAskName),
                    "dppgroup" => isDesc ? ordered.ThenByDescending(x => x.DppGroup) : ordered.ThenBy(x => x.DppGroup),
                    "levelneed" => isDesc ? ordered.ThenByDescending(x => x.LevelNeed) : ordered.ThenBy(x => x.LevelNeed),
                    "outgoingresource" => isDesc ? ordered.ThenByDescending(x => x.OutGoingResource) : ordered.ThenBy(x => x.OutGoingResource),
                    "needreason" => isDesc ? ordered.ThenByDescending(x => x.NeedReason) : ordered.ThenBy(x => x.NeedReason),
                    "fyear" => isDesc ? ordered.ThenByDescending(x => x.FYear) : ordered.ThenBy(x => x.FYear),
                    "projectedstartdate" => isDesc ? ordered.ThenByDescending(x => x.ProjectedStartDate) : ordered.ThenBy(x => x.ProjectedStartDate),
                    "enddate" => isDesc ? ordered.ThenByDescending(x => x.EndDate) : ordered.ThenBy(x => x.EndDate),
                    "generalspecialityneed" => isDesc ? ordered.ThenByDescending(x => x.GeneralSpecialityNeed) : ordered.ThenBy(x => x.GeneralSpecialityNeed),
                    "roleposting" => isDesc ? ordered.ThenByDescending(x => x.RolePosting) : ordered.ThenBy(x => x.RolePosting),
                    "version" => isDesc ? ordered.ThenByDescending(x => x.Version) : ordered.ThenBy(x => x.Version),
                    "iscompleted" => isDesc ? ordered.ThenByDescending(x => x.IsCompleted) : ordered.ThenBy(x => x.IsCompleted),
                    "statusid" => isDesc ? ordered.ThenByDescending(x => x.StatusId) : ordered.ThenBy(x => x.StatusId),
                    _ => ordered
                };
            }
        }

        return ordered ?? query.OrderByDescending(x => x.AskId);
    }
}

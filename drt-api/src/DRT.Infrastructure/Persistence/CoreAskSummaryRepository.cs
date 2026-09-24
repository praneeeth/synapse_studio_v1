using DRT.Application.Abstractions;
using DRT.Contracts.CoreAsks;
using Microsoft.EntityFrameworkCore;

namespace DRT.Infrastructure.Persistence;

/// <summary>
/// Read-only EF Core implementation of ICoreAskSummaryRepository.
/// Queries CAW_Ask (Ask) and CAW_CoreAskDetail (CoreAskDetail) with version filtering.
/// Uses AsNoTracking for all reads (read-only operation, BR-006).
/// TODO (OI-001): Reference data lookups (statusId→status, dppGroupId→dppGroup,
/// levelNeedId→levelNeed, needReasonId→needReason) are not yet implemented;
/// the display-name fields are left null until the lookup mechanism is confirmed.
/// TODO (A-004): requestNumber and numberOfResources column mappings are not yet confirmed;
/// they are projected from CoreAskDetail once the mapping is clarified.
/// </summary>
public sealed class CoreAskSummaryRepository : ICoreAskSummaryRepository
{
    private readonly ApplicationDbContext _db;

    public CoreAskSummaryRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<bool> AskExistsAsync(
        int askId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Asks
            .AsNoTracking()
            .AnyAsync(a => a.AskId == askId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CoreAskSummaryResponse?> GetSummaryAsync(
        int askId,
        int? version,
        CancellationToken cancellationToken = default)
    {
        // BR-001: when version is null, use the highest version value for the askId
        // BR-002: totalVersions = count of all CAW_CoreAskDetail rows for the askId
        var totalVersions = await _db.CoreAskDetails
            .AsNoTracking()
            .Where(d => d.AskId == askId)
            .CountAsync(cancellationToken);

        // Determine the target version
        int targetVersion;
        if (version.HasValue)
        {
            targetVersion = version.Value;
        }
        else
        {
            // BR-001: latest = highest version number
            var latestVersion = await _db.CoreAskDetails
                .AsNoTracking()
                .Where(d => d.AskId == askId)
                .MaxAsync(d => (int?)d.Version, cancellationToken);

            if (latestVersion is null)
                return null;

            targetVersion = latestVersion.Value;
        }

        // Join CAW_Ask and CAW_CoreAskDetail on askId + version
        var row = await (
            from ask in _db.Asks.AsNoTracking()
            join detail in _db.CoreAskDetails.AsNoTracking()
                on new { ask.AskId, Version = targetVersion }
                equals new { detail.AskId, detail.Version }
            where ask.AskId == askId
            select new
            {
                // Identifiers
                ask.AskId,
                detail.CoreAskDetailId,

                // Business fields from detail
                detail.CoreAskName,
                detail.FYear,
                detail.DppGroupId,
                detail.NeedReasonId,
                detail.LevelNeedId,
                detail.TitlingCategory,
                detail.Pml,
                detail.TransitionalCoach,
                detail.GeneralSpecialityNeedId,
                detail.GeneralSpecialityNeedComment,
                detail.RolePostingId,
                detail.OutgoingResource,
                detail.HeadCountAmount,
                detail.FteAmount,
                detail.EndDate,
                detail.ProjectedStartDate,
                detail.RoleSummary,
                detail.RoleResponsibility,
                detail.RoleQualification,
                detail.NumberOfResources,
                detail.Version,

                // State flags from ask
                ask.StatusId,
                ask.IsActive,
                ask.IsCompleted,

                // Audit metadata from ask
                ask.CreatedBy,
                ask.CreatedOn,
                ask.ModifiedBy,
                ask.ModifiedOn
            }
        ).FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return null;

        return new CoreAskSummaryResponse
        {
            AskId = row.AskId,
            AskDetailId = row.CoreAskDetailId,
            CoreAskName = row.CoreAskName,
            // TODO (OI-001): RecordName resolved from reference data – not yet implemented
            RecordName = null,
            FYear = row.FYear,
            StatusId = row.StatusId,
            // TODO (OI-001): Status display name resolved from statusId – not yet implemented
            Status = null,
            DppGroupId = row.DppGroupId,
            // TODO (OI-001): DppGroup display name resolved from dppGroupId – not yet implemented
            DppGroup = null,
            // TODO (OI-001): NeedReason display name resolved from needReasonId – not yet implemented
            NeedReason = null,
            LevelNeedId = row.LevelNeedId,
            // TODO (OI-001): LevelNeed display name resolved from levelNeedId – not yet implemented
            LevelNeed = null,
            TitlingCategory = row.TitlingCategory,
            Pml = row.Pml,
            TransitionalCoach = row.TransitionalCoach,
            // TODO (OI-001): GeneralSpecialityNeed display name resolved from generalSpecialityNeedId – not yet implemented
            GeneralSpecialityNeed = null,
            GeneralSpecialityNeedComment = row.GeneralSpecialityNeedComment,
            // TODO (OI-001): RolePosting display name resolved from rolePostingId – not yet implemented
            RolePosting = null,
            OutGoingResource = row.OutgoingResource,
            HeadCountAmount = row.HeadCountAmount,
            FteAmount = row.FteAmount,
            EndDate = row.EndDate,
            ProjectedStartDate = row.ProjectedStartDate == default ? null : row.ProjectedStartDate,
            RoleSummary = row.RoleSummary,
            RoleResponsibility = row.RoleResponsibility,
            RoleQualification = row.RoleQualification,
            // TODO (A-004): PostingReqNumber column mapping not yet confirmed
            PostingReqNumber = null,
            // TODO (A-004): RequestNumber column mapping not yet confirmed
            RequestNumber = null,
            NumberOfResources = row.NumberOfResources,
            Version = row.Version,
            TotalVersions = totalVersions,
            IsActive = row.IsActive,
            IsCompleted = row.IsCompleted,
            CreatedBy = row.CreatedBy,
            CreatedOn = row.CreatedOn,
            ModifiedBy = row.ModifiedBy,
            ModifiedOn = row.ModifiedOn
        };
    }
}

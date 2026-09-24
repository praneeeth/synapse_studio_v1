namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Response contract for GET /core-asks/{askId}/summary (Story 1854, US-ASK-003).
/// </summary>
public sealed class CoreAskSummaryResponse
{
    // Core Identifiers
    public int AskId { get; init; }
    public int AskDetailId { get; init; }

    // Business Fields
    public string? CoreAskName { get; init; }
    public string? RecordName { get; init; }
    public int? FYear { get; init; }
    public int? StatusId { get; init; }
    public string? Status { get; init; }
    public string? DppGroup { get; init; }
    public int? DppGroupId { get; init; }
    public string? NeedReason { get; init; }
    public string? LevelNeed { get; init; }
    public int? LevelNeedId { get; init; }

    /// <summary>
    /// BR-003: Applies only to new-version asks; may be null for other versions.
    /// TODO (OI-003): Clarify "new-version" criteria for titlingCategory applicability.
    /// </summary>
    public string? TitlingCategory { get; init; }

    /// <summary>
    /// BR-003: Applies only to new-version asks; may be null for other versions.
    /// TODO (OI-003): Clarify "new-version" criteria for pml applicability.
    /// </summary>
    public string? Pml { get; init; }

    /// <summary>
    /// BR-003: Applies only to new-version asks; may be null for other versions.
    /// TODO (OI-003): Clarify "new-version" criteria for transitionalCoach applicability.
    /// </summary>
    public string? TransitionalCoach { get; init; }

    public string? GeneralSpecialityNeed { get; init; }
    public string? GeneralSpecialityNeedComment { get; init; }
    public string? RolePosting { get; init; }
    public string? OutGoingResource { get; init; }
    public decimal? HeadCountAmount { get; init; }
    public decimal? FteAmount { get; init; }
    public DateOnly? EndDate { get; init; }
    public DateOnly? ProjectedStartDate { get; init; }
    public string? RoleSummary { get; init; }
    public string? RoleResponsibility { get; init; }
    public string? RoleQualification { get; init; }
    public string? PostingReqNumber { get; init; }

    /// <summary>
    /// TODO (A-004): Confirm mapping of requestNumber to database column.
    /// </summary>
    public string? RequestNumber { get; init; }

    /// <summary>
    /// TODO (A-004): Confirm mapping of numberOfResources to database column.
    /// </summary>
    public int? NumberOfResources { get; init; }

    // Version & State
    /// <summary>The version number of the returned record (BR-001).</summary>
    public int Version { get; init; }

    /// <summary>Total number of versions for this askId (BR-002).</summary>
    public int TotalVersions { get; init; }

    /// <summary>BR-004: Indicates whether this version is active.</summary>
    public bool IsActive { get; init; }

    /// <summary>BR-004: Indicates whether this version is completed.</summary>
    public bool IsCompleted { get; init; }

    // Audit Metadata
    public string? CreatedBy { get; init; }
    public DateTimeOffset? CreatedOn { get; init; }
    public string? ModifiedBy { get; init; }
    public DateTimeOffset? ModifiedOn { get; init; }
}

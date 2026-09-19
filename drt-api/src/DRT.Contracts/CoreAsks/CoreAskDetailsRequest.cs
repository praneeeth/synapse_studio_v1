namespace DRT.Contracts.CoreAsks;

public sealed class CoreAskDetailsRequest
{
    public string? CoreAskName { get; init; }
    public int? FYear { get; init; }
    public int? RegionId { get; init; }
    public int? DppGroupId { get; init; }
    public int? NeedReasonId { get; init; }
    public int? GeneralSpecialityNeedId { get; init; }
    public string? GeneralSpecialityNeedComment { get; init; }
    public int? LevelNeedId { get; init; }
    /// <summary>Optional, new-version only, max 99 characters.</summary>
    public string? Pml { get; init; }
    public DateOnly? ProjectedStartDate { get; init; }
    /// <summary>Conditionally required; not required when needReasonId is retirement.</summary>
    public DateOnly? EndDate { get; init; }
    /// <summary>Read-only; derived from EmployeeId. Must not be supplied by caller as authoritative.</summary>
    public string? OutgoingResource { get; init; }
    public int? EmployeeId { get; init; }
    public decimal? HeadCountAmount { get; init; }
    public decimal? FteAmount { get; init; }
    /// <summary>Conditionally required when LevelNeedId is in the top-3 set.</summary>
    public int? RolePostingId { get; init; }
    public int? NumberOfResources { get; init; }
    /// <summary>Required, new-version only.</summary>
    public string? TitlingCategory { get; init; }
    /// <summary>Optional, new-version only, max 99 characters.</summary>
    public string? TransitionalCoach { get; init; }
    public string? RoleSummary { get; init; }
    public string? RoleResponsibility { get; init; }
    public string? RoleQualification { get; init; }
}

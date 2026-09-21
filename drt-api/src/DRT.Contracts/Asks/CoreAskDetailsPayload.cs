namespace DRT.Contracts.Asks;

/// <summary>
/// Core ASK-specific fields carried inside <see cref="CreateAskRequest"/> when AskType is "Core".
/// </summary>
public sealed class CoreAskDetailsPayload
{
    public string? CoreAskName { get; init; }
    public int? FYear { get; init; }
    public int? RegionId { get; init; }
    public int? DppGroupId { get; init; }
    public int? NeedReasonId { get; init; }
    public int? GeneralSpecialityNeedId { get; init; }
    public string? GeneralSpecialityNeedComment { get; init; }
    public int? LevelNeedId { get; init; }
    public string? Pml { get; init; }
    public DateOnly? ProjectedStartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public int? EmployeeId { get; init; }
    public decimal? HeadCountAmount { get; init; }
    public decimal? FteAmount { get; init; }
    public int? RolePostingId { get; init; }
    public int? NumberOfResources { get; init; }
    public string? TitlingCategory { get; init; }
    public string? TransitionalCoach { get; init; }
    public string? RoleSummary { get; init; }
    public string? RoleResponsibility { get; init; }
    public string? RoleQualification { get; init; }
}

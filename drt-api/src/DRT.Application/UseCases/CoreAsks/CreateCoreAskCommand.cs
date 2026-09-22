namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Command for creating a new Core ASK.
/// </summary>
public sealed class CreateCoreAskCommand
{
    public int ActorId { get; init; }
    public bool ActorIsLeadership { get; init; }
    public string CoreAskName { get; init; } = string.Empty;
    public int FYear { get; init; }
    public int RegionId { get; init; }
    public int DppGroupId { get; init; }
    public int NeedReasonId { get; init; }
    public int GeneralSpecialityNeedId { get; init; }
    public string? GeneralSpecialityNeedComment { get; init; }
    public int LevelNeedId { get; init; }
    public string? Pml { get; init; }
    public DateOnly ProjectedStartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public int? EmployeeId { get; init; }
    public decimal HeadCountAmount { get; init; }
    public decimal FteAmount { get; init; }
    public int? RolePostingId { get; init; }
    public int? NumberOfResources { get; init; }
    public string? TitlingCategory { get; init; }
    public string? TransitionalCoach { get; init; }
    public string RoleSummary { get; init; } = string.Empty;
    public string RoleResponsibility { get; init; } = string.Empty;
    public string RoleQualification { get; init; } = string.Empty;
    public string? Comment { get; init; }
    /// <summary>
    /// Attachment identifiers already uploaded via the attachment service.
    /// TODO: Confirm exact attachment identifier contract (open item: Attachment Upload Service Contract).
    /// </summary>
    public IReadOnlyList<string>? AttachmentIdentifiers { get; init; }
    /// <summary>Allowed: "Save &amp; Exit", "SUBMIT", "Exit".</summary>
    public string ButtonValue { get; init; } = string.Empty;
}

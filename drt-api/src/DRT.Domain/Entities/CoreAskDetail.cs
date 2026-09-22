namespace DRT.Domain.Entities;

/// <summary>
/// Core ASK-specific detail fields.
/// </summary>
public sealed class CoreAskDetail
{
    public int CoreAskDetailId { get; private set; }
    public int AskId { get; private set; }
    public string CoreAskName { get; private set; } = string.Empty;
    public int FYear { get; private set; }
    public int RegionId { get; private set; }
    public int DppGroupId { get; private set; }
    public int NeedReasonId { get; private set; }
    public int GeneralSpecialityNeedId { get; private set; }
    public string? GeneralSpecialityNeedComment { get; private set; }
    public int LevelNeedId { get; private set; }
    public string? Pml { get; private set; }
    public DateOnly ProjectedStartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? OutgoingResource { get; private set; }
    public int? EmployeeId { get; private set; }
    public decimal HeadCountAmount { get; private set; }
    public decimal FteAmount { get; private set; }
    public int? RolePostingId { get; private set; }
    public int? NumberOfResources { get; private set; }
    public string? TitlingCategory { get; private set; }
    public string? TransitionalCoach { get; private set; }
    public string RoleSummary { get; private set; } = string.Empty;
    public string RoleResponsibility { get; private set; } = string.Empty;
    public string RoleQualification { get; private set; } = string.Empty;

    private CoreAskDetail() { }

    public static CoreAskDetail Create(
        int askId,
        string coreAskName,
        int fYear,
        int regionId,
        int dppGroupId,
        int needReasonId,
        int generalSpecialityNeedId,
        string? generalSpecialityNeedComment,
        int levelNeedId,
        string? pml,
        DateOnly projectedStartDate,
        DateOnly? endDate,
        string? outgoingResource,
        int? employeeId,
        decimal headCountAmount,
        decimal fteAmount,
        int? rolePostingId,
        int? numberOfResources,
        string? titlingCategory,
        string? transitionalCoach,
        string roleSummary,
        string roleResponsibility,
        string roleQualification)
    {
        return new CoreAskDetail
        {
            AskId = askId,
            CoreAskName = coreAskName,
            FYear = fYear,
            RegionId = regionId,
            DppGroupId = dppGroupId,
            NeedReasonId = needReasonId,
            GeneralSpecialityNeedId = generalSpecialityNeedId,
            GeneralSpecialityNeedComment = generalSpecialityNeedComment,
            LevelNeedId = levelNeedId,
            Pml = pml,
            ProjectedStartDate = projectedStartDate,
            EndDate = endDate,
            OutgoingResource = outgoingResource,
            EmployeeId = employeeId,
            HeadCountAmount = headCountAmount,
            FteAmount = fteAmount,
            RolePostingId = rolePostingId,
            NumberOfResources = numberOfResources,
            TitlingCategory = titlingCategory,
            TransitionalCoach = transitionalCoach,
            RoleSummary = roleSummary,
            RoleResponsibility = roleResponsibility,
            RoleQualification = roleQualification
        };
    }
}

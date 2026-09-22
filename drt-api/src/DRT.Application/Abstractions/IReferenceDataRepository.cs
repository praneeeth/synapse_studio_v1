namespace DRT.Application.Abstractions;

/// <summary>
/// Port for validating reference data options.
/// TODO: Confirm exact service/repository method for validating active reference IDs (open item: Reference Data Validation Service).
/// </summary>
public interface IReferenceDataRepository
{
    Task<bool> IsDppGroupActiveAsync(int dppGroupId, CancellationToken cancellationToken = default);
    Task<bool> IsNeedReasonActiveAsync(int needReasonId, CancellationToken cancellationToken = default);
    Task<bool> IsGeneralSpecialityNeedActiveAsync(int generalSpecialityNeedId, CancellationToken cancellationToken = default);
    Task<bool> IsLevelNeedActiveAsync(int levelNeedId, CancellationToken cancellationToken = default);
    Task<bool> IsRolePostingActiveAsync(int rolePostingId, CancellationToken cancellationToken = default);
}

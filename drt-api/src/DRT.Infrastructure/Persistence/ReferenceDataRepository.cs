using DRT.Application.Abstractions;
using DRT.Domain.Entities;

namespace DRT.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for reference data validation.
/// TODO: Confirm exact service/repository method for validating active reference IDs (open item: Reference Data Validation Service).
/// </summary>
public sealed class ReferenceDataRepository : IReferenceDataRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ReferenceDataRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsDppGroupActiveAsync(int dppGroupId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement query against DppGroup reference table once schema is confirmed.
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> IsNeedReasonActiveAsync(int needReasonId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement query against NeedReason reference table once schema is confirmed.
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> IsGeneralSpecialityNeedActiveAsync(int generalSpecialityNeedId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement query against GeneralSpecialityNeed reference table once schema is confirmed.
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> IsLevelNeedActiveAsync(int levelNeedId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement query against LevelNeed reference table once schema is confirmed.
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> IsRolePostingActiveAsync(int rolePostingId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement query against RolePosting reference table once schema is confirmed.
        await Task.CompletedTask;
        return true;
    }
}

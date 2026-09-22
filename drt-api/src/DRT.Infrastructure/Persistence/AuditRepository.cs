using DRT.Application.Abstractions;
using DRT.Domain.Entities;

namespace DRT.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for audit records.
/// </summary>
public sealed class AuditRepository : IAuditRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AuditRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAuditRecordAsync(AuditRecord record, CancellationToken cancellationToken = default)
        => await _dbContext.AuditRecords.AddAsync(record, cancellationToken);
}

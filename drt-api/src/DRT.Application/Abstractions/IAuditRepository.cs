using DRT.Domain.Entities;

namespace DRT.Application.Abstractions;

/// <summary>
/// Port for persisting audit records.
/// </summary>
public interface IAuditRepository
{
    Task AddAuditRecordAsync(AuditRecord record, CancellationToken cancellationToken = default);
}

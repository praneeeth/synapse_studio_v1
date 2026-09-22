namespace DRT.Application.Abstractions;

/// <summary>
/// Port for committing the unit of work (single SQL transaction).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

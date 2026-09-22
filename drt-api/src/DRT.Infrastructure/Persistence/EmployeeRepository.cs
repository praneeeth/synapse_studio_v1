using DRT.Application.Abstractions;

namespace DRT.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for employee lookup.
/// TODO: Confirm exact API contract or repository method for employee lookup (open item: Employee Lookup Service Contract).
/// </summary>
public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EmployeeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> ResolveOutgoingResourceAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement employee lookup query once employee entity/service contract is confirmed.
        // TODO: Clarify fallback behavior when employeeId does not resolve (open item: Employee Lookup Fallback Behavior).
        await Task.CompletedTask;
        return null;
    }
}

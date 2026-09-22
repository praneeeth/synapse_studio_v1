namespace DRT.Application.Abstractions;

/// <summary>
/// Port for resolving employee data.
/// TODO: Confirm exact API contract or repository method for employee lookup (open item: Employee Lookup Service Contract).
/// </summary>
public interface IEmployeeRepository
{
    /// <summary>
    /// Resolves the outgoing resource name from the given employee ID.
    /// Returns null if the employee is not found.
    /// TODO: Clarify fallback behavior when employeeId does not resolve (open item: Employee Lookup Fallback Behavior).
    /// </summary>
    Task<string?> ResolveOutgoingResourceAsync(int employeeId, CancellationToken cancellationToken = default);
}

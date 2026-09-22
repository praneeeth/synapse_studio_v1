namespace DRT.Domain.Entities;

/// <summary>
/// Thrown when a requested ASK cannot be found.
/// Maps to HTTP 404 Not Found with error code resource_not_found.
/// </summary>
public sealed class AskNotFoundException : Exception
{
    public int AskId { get; }

    public AskNotFoundException(int askId)
        : base($"Core ASK with ID {askId} not found.")
    {
        AskId = askId;
    }
}

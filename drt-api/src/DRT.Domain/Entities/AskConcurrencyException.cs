namespace DRT.Domain.Entities;

/// <summary>
/// Thrown by the repository when an EF Core concurrency conflict is detected
/// on SaveChangesAsync (version/RowVersion mismatch on the Ask aggregate).
/// Keeps the Application layer free of EF Core dependencies.
/// </summary>
public sealed class AskConcurrencyException : Exception
{
    public AskConcurrencyException()
        : base("The ASK has been modified by another user.") { }

    public AskConcurrencyException(string message)
        : base(message) { }

    public AskConcurrencyException(string message, Exception innerException)
        : base(message, innerException) { }
}

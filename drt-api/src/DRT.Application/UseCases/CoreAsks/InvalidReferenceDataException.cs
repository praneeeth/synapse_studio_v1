namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Thrown when a reference data ID is not an active/valid option.
/// Maps to HTTP 400 Bad Request.
/// </summary>
public sealed class InvalidReferenceDataException : Exception
{
    public InvalidReferenceDataException(string message) : base(message) { }
}

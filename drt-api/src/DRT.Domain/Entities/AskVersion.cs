namespace DRT.Domain.Entities;

/// <summary>
/// Version tracking entity for ASK mutations.
/// </summary>
public sealed class AskVersion
{
    public int AskVersionId { get; private set; }
    public int AskId { get; private set; }
    public int VersionNumber { get; private set; }
    public DateTimeOffset CreatedUtc { get; private set; }
    public int CreatedByActorId { get; private set; }

    private AskVersion() { }

    public static AskVersion Create(int askId, int versionNumber, int createdByActorId, DateTimeOffset createdUtc)
    {
        return new AskVersion
        {
            AskId = askId,
            VersionNumber = versionNumber,
            CreatedByActorId = createdByActorId,
            CreatedUtc = createdUtc
        };
    }
}

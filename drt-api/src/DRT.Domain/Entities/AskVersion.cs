namespace DRT.Domain.Entities;

/// <summary>
/// Version record for an ASK mutation.
/// </summary>
public sealed class AskVersion
{
    public int Id { get; private set; }
    public int AskId { get; private set; }
    public int VersionNumber { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public int CreatedBy { get; private set; }

    private AskVersion() { }

    public static AskVersion Create(int askId, int versionNumber, int actorId, DateTimeOffset now)
    {
        return new AskVersion
        {
            AskId = askId,
            VersionNumber = versionNumber,
            CreatedAt = now,
            CreatedBy = actorId
        };
    }
}

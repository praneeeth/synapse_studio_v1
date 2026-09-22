namespace DRT.Domain.Entities;

/// <summary>
/// Primary ASK aggregate root entity.
/// </summary>
public sealed class Ask
{
    public int AskId { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset CreatedUtc { get; private set; }
    public int CreatedByActorId { get; private set; }

    private Ask() { }

    public static Ask Create(int createdByActorId, DateTimeOffset createdUtc)
    {
        return new Ask
        {
            Version = 1,
            CreatedByActorId = createdByActorId,
            CreatedUtc = createdUtc
        };
    }
}

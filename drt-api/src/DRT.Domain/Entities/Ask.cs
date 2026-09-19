namespace DRT.Domain.Entities;

/// <summary>
/// Primary ASK aggregate root.
/// </summary>
public sealed class Ask
{
    public int Id { get; private set; }
    public AskStatus Status { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public int CreatedBy { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int UpdatedBy { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // EF Core constructor
    private Ask() { }

    public static Ask Create(AskStatus initialStatus, int actorId, DateTimeOffset now)
    {
        return new Ask
        {
            Status = initialStatus,
            Version = 1,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId
        };
    }
}

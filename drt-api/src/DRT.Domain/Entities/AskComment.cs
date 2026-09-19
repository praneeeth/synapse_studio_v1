namespace DRT.Domain.Entities;

/// <summary>
/// Optional comment associated with an ASK.
/// </summary>
public sealed class AskComment
{
    public int Id { get; private set; }
    public int AskId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public int CreatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private AskComment() { }

    public static AskComment Create(int askId, string text, int actorId, DateTimeOffset now)
    {
        return new AskComment
        {
            AskId = askId,
            Text = text,
            CreatedBy = actorId,
            CreatedAt = now
        };
    }
}

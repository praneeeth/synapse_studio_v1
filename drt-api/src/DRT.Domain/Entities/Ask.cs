namespace DRT.Domain.Entities;

/// <summary>
/// Primary ASK aggregate root entity.
/// Supports creation and terminal cancellation transitions.
/// </summary>
public sealed class Ask
{
    /// <summary>Status ID: Cancelled (terminal).</summary>
    public const int StatusCancelled = 577;

    /// <summary>Status ID: Completed (terminal).</summary>
    public const int StatusCompleted = 135;

    /// <summary>Status ID: In Progress (draft saved).</summary>
    public const int StatusInProgress = 1; // TODO: confirm exact In-Progress status ID

    public int AskId { get; private set; }
    public int Version { get; private set; }
    public int StatusId { get; private set; }
    public DateTimeOffset CreatedUtc { get; private set; }
    public int CreatedByActorId { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancelledBy { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTimeOffset? ModifiedOn { get; private set; }

    /// <summary>EF Core concurrency token mapped to SQL rowversion.</summary>
    public byte[]? RowVersion { get; private set; }

    private Ask() { }

    public static Ask Create(int createdByActorId, DateTimeOffset createdUtc)
    {
        return new Ask
        {
            Version = 1,
            StatusId = StatusInProgress,
            CreatedByActorId = createdByActorId,
            CreatedUtc = createdUtc
        };
    }

    /// <summary>
    /// Applies the terminal Cancel transition.
    /// Throws <see cref="AskInvalidStateException"/> when the ASK is already Cancelled (577)
    /// or Completed (135).
    /// </summary>
    public void Cancel(string cancelledBy, int actorId, DateTimeOffset cancelledAt)
    {
        if (StatusId == StatusCancelled)
            throw new AskInvalidStateException(AskId, StatusId, "Core ASK is already cancelled.");

        if (StatusId == StatusCompleted)
            throw new AskInvalidStateException(AskId, StatusId, "Core ASK is already completed and cannot be cancelled.");

        StatusId = StatusCancelled;
        CancelledAt = cancelledAt;
        CancelledBy = cancelledBy;
        ModifiedBy = cancelledBy;
        ModifiedOn = cancelledAt;
        Version += 1;
    }
}

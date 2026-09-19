namespace DRT.Domain.Entities;

/// <summary>
/// Primary ASK aggregate root.
/// </summary>
public sealed class Ask
{
    public int Id { get; private set; }
    public AskStatus Status { get; private set; }

    /// <summary>Numeric status identifier; kept in sync with Status enum.</summary>
    public int StatusId { get; private set; }

    public int Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public int CreatedBy { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int UpdatedBy { get; private set; }

    // Cancellation fields (US-ASK-015)
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancelledBy { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTimeOffset? ModifiedOn { get; private set; }

    /// <summary>EF Core concurrency token.</summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // EF Core constructor
    private Ask() { }

    public static Ask Create(AskStatus initialStatus, int actorId, DateTimeOffset now)
    {
        return new Ask
        {
            Status = initialStatus,
            StatusId = (int)initialStatus,
            Version = 1,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId
        };
    }

    /// <summary>
    /// Transitions the ASK to Cancelled state (statusId 577).
    /// Called by CancelCoreAskUseCase (US-ASK-015).
    /// Precondition: caller must have already validated that statusId is not 577 or 135.
    /// </summary>
    public void Cancel(string cancelledByIdentity, DateTimeOffset now)
    {
        const int cancelledStatusId = 577;

        StatusId = cancelledStatusId;
        // Status enum does not include 577 (terminal, not in original workflow enum).
        // StatusId is the authoritative field for persistence; Status enum is for in-process routing only.
        CancelledAt = now;
        CancelledBy = cancelledByIdentity;
        ModifiedBy = cancelledByIdentity;
        ModifiedOn = now;
        UpdatedAt = now;
        Version += 1;
    }
}

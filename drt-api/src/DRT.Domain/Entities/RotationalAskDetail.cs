namespace DRT.Domain.Entities;

/// <summary>
/// Rotational ASK-specific detail fields.
/// Stores the per-BU plan entries and the name of the ASK.
/// </summary>
public sealed class RotationalAskDetail
{
    public int Id { get; private set; }
    public int AskId { get; private set; }

    /// <summary>
    /// Name of the ASK. Required for Rotational ASK. Max 100 characters.
    /// Added in RT_August_Release.
    /// </summary>
    public string? NameOfTheAsk { get; private set; }

    /// <summary>Calendar year of the Rotational ASK.</summary>
    public int? Year { get; private set; }

    // EF Core constructor
    private RotationalAskDetail() { }

    public static RotationalAskDetail Create(
        int askId,
        string? nameOfTheAsk,
        int? year)
    {
        return new RotationalAskDetail
        {
            AskId = askId,
            NameOfTheAsk = nameOfTheAsk,
            Year = year
        };
    }
}

namespace DRT.Domain.Entities;

/// <summary>
/// Approved workflow status values for an ASK.
/// Status code values sourced from US-ASK-001.
/// TODO: Confirm status codes 123, 127, 145 against the DRT workflow status reference data (Open Item #1).
/// </summary>
public enum AskStatus
{
    /// <summary>Draft state. Status code 123.</summary>
    InProgress = 123,

    /// <summary>Routed to PPL Review. Status code 127.</summary>
    PplReview = 127,

    /// <summary>Routed to DPP Ops Review (leadership submitter path). Status code 145.</summary>
    DppOpsReview = 145
}

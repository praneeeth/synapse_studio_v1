namespace DRT.Workflow;

/// <summary>
/// Workflow status IDs used for Core ASK routing.
/// TODO: Confirm that status codes 123, 127, 145 align with workflow status reference data (open item: Status Code Confirmation).
/// </summary>
public static class CoreAskWorkflowStatus
{
    /// <summary>Draft / In Progress status for Save &amp; Exit.</summary>
    public const int InProgress = 123;

    /// <summary>PPL Review status for SUBMIT by non-leadership actor.</summary>
    public const int PplReview = 127;

    /// <summary>DPP Ops Review status for SUBMIT by leadership actor.</summary>
    public const int DppOpsReview = 145;
}

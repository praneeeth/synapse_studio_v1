using FluentValidation;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Validates the CreateCoreAskCommand field-level and business rules.
/// Open items are noted with TODO where exact reference IDs are unknown.
/// </summary>
public sealed class CreateCoreAskCommandValidator : AbstractValidator<CreateCoreAskCommand>
{
    /// <summary>
    /// TODO: Confirm the exact NeedReasonId that represents "first option" (open item: First Option Identification).
    /// </summary>
    private const int NeedReasonFirstOptionId = 0; // TODO: replace with confirmed reference ID

    /// <summary>
    /// TODO: Confirm the exact NeedReasonId that represents "retirement" (open item: Retirement Need Reason ID).
    /// </summary>
    private const int NeedReasonRetirementId = 0; // TODO: replace with confirmed reference ID

    /// <summary>
    /// TODO: Confirm the exact GeneralSpecialityNeedId that represents "first option" (open item: First Option Identification).
    /// </summary>
    private const int GeneralSpecialityNeedFirstOptionId = 0; // TODO: replace with confirmed reference ID

    /// <summary>
    /// TODO: Confirm the exact set of LevelNeedIds that constitute the "top-3 set" requiring rolePostingId (open item: Top-3 Level Need Set Definition).
    /// </summary>
    private static readonly IReadOnlySet<int> LevelNeedTopThreeSet = new HashSet<int>(); // TODO: populate with confirmed IDs

    public CreateCoreAskCommandValidator()
    {
        // ButtonValue must be one of the allowed values
        RuleFor(x => x.ButtonValue)
            .Must(v => v == ButtonValues.SaveAndExit || v == ButtonValues.Submit || v == ButtonValues.Exit)
            .WithMessage("buttonValue must be one of: 'Save & Exit', 'SUBMIT', 'Exit'.");

        // Rules only apply when not Exit
        When(x => x.ButtonValue != ButtonValues.Exit, () =>
        {
            RuleFor(x => x.CoreAskName)
                .NotEmpty().WithMessage("Field CoreAskName is required")
                .MaximumLength(200).WithMessage("Field CoreAskName exceeds maximum length of 200");

            RuleFor(x => x.DppGroupId)
                .GreaterThan(0).WithMessage("Field DppGroupId is required");

            RuleFor(x => x.NeedReasonId)
                .GreaterThan(0).WithMessage("Field NeedReasonId is required");

            RuleFor(x => x.GeneralSpecialityNeedId)
                .GreaterThan(0).WithMessage("Field GeneralSpecialityNeedId is required");

            RuleFor(x => x.LevelNeedId)
                .GreaterThan(0).WithMessage("Field LevelNeedId is required");

            RuleFor(x => x.HeadCountAmount)
                .GreaterThan(0).WithMessage("Field HeadCountAmount is required");

            RuleFor(x => x.FteAmount)
                .GreaterThan(0).WithMessage("Field FteAmount is required");

            RuleFor(x => x.RoleSummary)
                .NotEmpty().WithMessage("Field RoleSummary is required");

            RuleFor(x => x.RoleResponsibility)
                .NotEmpty().WithMessage("Field RoleResponsibility is required");

            RuleFor(x => x.RoleQualification)
                .NotEmpty().WithMessage("Field RoleQualification is required");

            RuleFor(x => x.TitlingCategory)
                .NotEmpty().WithMessage("Field TitlingCategory is required");

            // pml max 99
            RuleFor(x => x.Pml)
                .MaximumLength(99).WithMessage("Field Pml exceeds maximum length of 99")
                .When(x => x.Pml != null);

            // transitionalCoach max 99
            RuleFor(x => x.TransitionalCoach)
                .MaximumLength(99).WithMessage("Field TransitionalCoach exceeds maximum length of 99")
                .When(x => x.TransitionalCoach != null);

            // fteAmount must not exceed headCountAmount
            RuleFor(x => x.FteAmount)
                .LessThanOrEqualTo(x => x.HeadCountAmount)
                .WithMessage("fteAmount must not exceed headCountAmount")
                .WithErrorCode("422");

            // projectedStartDate required (non-default) when needReasonId is NOT first option
            When(x => x.NeedReasonId != NeedReasonFirstOptionId, () =>
            {
                RuleFor(x => x.ProjectedStartDate)
                    .NotEqual(default(DateOnly)).WithMessage("Field ProjectedStartDate is required");
            });

            // endDate required unless needReasonId is retirement or first option
            When(x => x.NeedReasonId != NeedReasonRetirementId && x.NeedReasonId != NeedReasonFirstOptionId, () =>
            {
                RuleFor(x => x.EndDate)
                    .NotNull().WithMessage("Field EndDate is required");

                When(x => x.EndDate.HasValue, () =>
                {
                    RuleFor(x => x.EndDate!.Value)
                        .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
                        .WithMessage("endDate must not be earlier than today")
                        .WithErrorCode("422");

                    RuleFor(x => x.EndDate!.Value)
                        .GreaterThan(x => x.ProjectedStartDate)
                        .WithMessage("endDate must be after projectedStartDate")
                        .WithErrorCode("422");
                });
            });

            // generalSpecialityNeedComment required unless generalSpecialityNeedId is first option
            When(x => x.GeneralSpecialityNeedId != GeneralSpecialityNeedFirstOptionId, () =>
            {
                RuleFor(x => x.GeneralSpecialityNeedComment)
                    .NotEmpty().WithMessage("Field GeneralSpecialityNeedComment is required");
            });

            // rolePostingId required when levelNeedId is in top-3 set
            When(x => LevelNeedTopThreeSet.Contains(x.LevelNeedId), () =>
            {
                RuleFor(x => x.RolePostingId)
                    .NotNull().WithMessage("Field RolePostingId is required");
            });
        });
    }
}

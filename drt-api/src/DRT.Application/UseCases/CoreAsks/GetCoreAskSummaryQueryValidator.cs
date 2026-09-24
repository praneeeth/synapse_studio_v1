using FluentValidation;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Validates the GetCoreAskSummaryQuery field-level rules (Story 1854, US-ASK-003).
/// VR-001: askId must be a valid (positive) integer.
/// VR-002: version, if supplied, must be a valid (positive) integer.
/// </summary>
public sealed class GetCoreAskSummaryQueryValidator : AbstractValidator<GetCoreAskSummaryQuery>
{
    public GetCoreAskSummaryQueryValidator()
    {
        // VR-001: askId must be a positive integer
        RuleFor(x => x.AskId)
            .GreaterThan(0)
            .WithMessage("askId must be a positive integer.");

        // VR-002: version, when provided, must be a positive integer
        When(x => x.Version.HasValue, () =>
        {
            RuleFor(x => x.Version!.Value)
                .GreaterThan(0)
                .WithMessage("version must be a positive integer.");
        });
    }
}

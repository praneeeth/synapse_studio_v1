using FluentValidation;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Validates the CancelCoreAskCommand field-level rules.
/// </summary>
public sealed class CancelCoreAskCommandValidator : AbstractValidator<CancelCoreAskCommand>
{
    public CancelCoreAskCommandValidator()
    {
        RuleFor(x => x.AskId)
            .GreaterThan(0)
            .WithMessage("askId must be a positive integer.");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("Comment is required for cancellation.");
    }
}

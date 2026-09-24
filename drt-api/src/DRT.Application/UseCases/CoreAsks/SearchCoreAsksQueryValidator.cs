using DRT.Contracts.CoreAsks;
using FluentValidation;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Validates the SearchCoreAsksQuery field-level rules (Story 1859).
/// BR-004: page and pageSize must be positive integers; pageSize capped by configuration.
/// </summary>
public sealed class SearchCoreAsksQueryValidator : AbstractValidator<SearchCoreAsksQuery>
{
    /// <summary>
    /// Maximum allowed pageSize.
    /// TODO: Confirm the exact maximum pageSize configuration value for production (OI-001).
    /// </summary>
    public const int MaxPageSize = 200; // TODO: replace with confirmed platform configuration value

    /// <summary>
    /// Allowlisted sort field names (BR-005).
    /// </summary>
    private static readonly IReadOnlySet<string> AllowedSortFields = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase)
    {
        "askId",
        "requestNumberDisplay",
        "coreAskName",
        "dppGroup",
        "levelNeed",
        "outGoingResource",
        "needReason",
        "fYear",
        "projectedStartDate",
        "endDate",
        "generalSpecialityNeed",
        "rolePosting",
        "version",
        "isCompleted",
        "statusId"
    };

    private static readonly IReadOnlySet<string> AllowedSortDirections = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase)
    {
        "asc",
        "desc"
    };

    public SearchCoreAsksQueryValidator()
    {
        // BR-004: page must be a positive integer
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("page must be a positive integer.");

        // BR-004: pageSize must be a positive integer within configured maximum
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("pageSize must be a positive integer.")
            .LessThanOrEqualTo(MaxPageSize)
            .WithMessage($"pageSize must not exceed {MaxPageSize}.");

        // Sort field and direction validation
        RuleForEach(x => x.Sort)
            .ChildRules(sort =>
            {
                sort.RuleFor(s => s.Field)
                    .NotEmpty()
                    .WithMessage("sort.field must not be empty.")
                    .Must(f => AllowedSortFields.Contains(f))
                    .WithMessage(s => $"sort.field '{s.Field}' is not a supported sort field.");

                sort.RuleFor(s => s.Direction)
                    .NotEmpty()
                    .WithMessage("sort.direction must not be empty.")
                    .Must(d => AllowedSortDirections.Contains(d))
                    .WithMessage(s => $"sort.direction '{s.Direction}' must be 'asc' or 'desc'.");
            })
            .When(x => x.Sort != null && x.Sort.Count > 0);
    }
}

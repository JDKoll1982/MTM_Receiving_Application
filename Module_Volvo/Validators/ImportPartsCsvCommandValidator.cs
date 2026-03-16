using FluentValidation;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Validators;

/// <summary>
/// Validates ImportPartsCommand before the handler runs.
/// </summary>
public class ImportPartsCommandValidator : AbstractValidator<ImportPartsCommand>
{
    public ImportPartsCommandValidator()
    {
        RuleFor(x => x.Parts)
            .NotEmpty().WithMessage("At least one part is required");

        RuleForEach(x => x.Parts).ChildRules(item =>
        {
            item.RuleFor(p => p.PartNumber)
                .NotEmpty().WithMessage("Part number is required")
                .MaximumLength(20).WithMessage("Part number must not exceed 20 characters");

            item.RuleFor(p => p.QuantityPerSkid)
                .GreaterThan(0).WithMessage("Quantity per skid must be greater than 0");
        });
    }
}

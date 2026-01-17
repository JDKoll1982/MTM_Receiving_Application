using FluentValidation;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Validators;

/// <summary>
/// Validator for AddVolvoPartCommand.
/// </summary>
public class AddVolvoPartCommandValidator : AbstractValidator<AddVolvoPartCommand>
{
    public AddVolvoPartCommandValidator()
    {
        RuleFor(x => x.PartNumber)
            .NotEmpty().WithMessage("Part number is required")
            .MaximumLength(50).WithMessage("Part number must not exceed 50 characters");

        RuleFor(x => x.QuantityPerSkid)
            .GreaterThan(0).WithMessage("Quantity per skid must be greater than 0");
    }
}

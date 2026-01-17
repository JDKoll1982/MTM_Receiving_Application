using FluentValidation;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Validators;

/// <summary>
/// Validator for DeactivateVolvoPartCommand.
/// </summary>
public class DeactivateVolvoPartCommandValidator : AbstractValidator<DeactivateVolvoPartCommand>
{
    public DeactivateVolvoPartCommandValidator()
    {
        RuleFor(x => x.PartNumber)
            .NotEmpty().WithMessage("Part number is required")
            .MaximumLength(50).WithMessage("Part number must not exceed 50 characters");
    }
}

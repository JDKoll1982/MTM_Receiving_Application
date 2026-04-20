using FluentValidation;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Validators;

/// <summary>
/// Validator for ActivateVolvoPartCommand.
/// </summary>
public class ActivateVolvoPartCommandValidator : AbstractValidator<ActivateVolvoPartCommand>
{
    public ActivateVolvoPartCommandValidator()
    {
        RuleFor(command => command.PartNumber).NotEmpty().MaximumLength(20);
    }
}

using FluentValidation;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Validators;

/// <summary>
/// Validator for SavePendingShipmentCommand.
/// </summary>
public class SavePendingShipmentCommandValidator : AbstractValidator<SavePendingShipmentCommand>
{
    public SavePendingShipmentCommandValidator()
    {
        RuleFor(x => x.ShipmentDate)
            .NotEmpty().WithMessage("Shipment date is required")
            .LessThanOrEqualTo(System.DateTimeOffset.Now).WithMessage("Shipment date cannot be in the future");

        RuleFor(x => x.Parts)
            .NotEmpty().WithMessage("At least one part is required");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
    }
}

using FluentValidation;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Validators;

/// <summary>
/// Validator for UpdateShipmentCommand.
/// </summary>
public class UpdateShipmentCommandValidator : AbstractValidator<UpdateShipmentCommand>
{
    public UpdateShipmentCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .GreaterThan(0).WithMessage("Shipment ID must be greater than 0");

        RuleFor(x => x.ShipmentDate)
            .NotEmpty().WithMessage("Shipment date is required")
            .LessThanOrEqualTo(System.DateTimeOffset.Now).WithMessage("Shipment date cannot be in the future");

        RuleFor(x => x.Parts)
            .NotEmpty().WithMessage("At least one part is required");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");

        RuleFor(x => x.PONumber)
            .MaximumLength(50).WithMessage("PO number must not exceed 50 characters");

        RuleFor(x => x.ReceiverNumber)
            .MaximumLength(50).WithMessage("Receiver number must not exceed 50 characters");

        RuleForEach(x => x.Parts).ChildRules(part =>
        {
            part.RuleFor(p => p.PartNumber)
                .NotEmpty().WithMessage("Part number is required");

            part.RuleFor(p => p.ReceivedSkidCount)
                .GreaterThan(0).WithMessage("Received skid count must be greater than 0");
        });
    }
}

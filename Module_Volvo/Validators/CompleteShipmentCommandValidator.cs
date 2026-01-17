using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Validators;

/// <summary>
/// Validator for CompleteShipmentCommand.
/// </summary>
public class CompleteShipmentCommandValidator : AbstractValidator<CompleteShipmentCommand>
{
    private readonly Dao_VolvoShipment _shipmentDao;

    public CompleteShipmentCommandValidator(Dao_VolvoShipment shipmentDao)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));

        RuleFor(x => x.ShipmentDate)
            .NotEmpty().WithMessage("Shipment date is required")
            .LessThanOrEqualTo(System.DateTimeOffset.Now).WithMessage("Shipment date cannot be in the future");

        RuleFor(x => x.Parts)
            .NotEmpty().WithMessage("At least one part is required");

        RuleFor(x => x.PONumber)
            .NotEmpty().WithMessage("PO number is required")
            .MaximumLength(50).WithMessage("PO number must not exceed 50 characters");

        RuleFor(x => x.ReceiverNumber)
            .NotEmpty().WithMessage("Receiver number is required")
            .MaximumLength(50).WithMessage("Receiver number must not exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");

        // Validate each part in the Parts collection
        RuleForEach(x => x.Parts).ChildRules(part =>
        {
            part.RuleFor(p => p.PartNumber)
                .NotEmpty().WithMessage("Part number is required");

            part.RuleFor(p => p.ReceivedSkidCount)
                .GreaterThan(0).WithMessage("Received skid count must be greater than 0");
        });

        RuleFor(x => x)
            .MustAsync(HasPendingShipmentAsync)
            .WithMessage("No pending shipment found to complete.");
    }

    private async Task<bool> HasPendingShipmentAsync(
        CompleteShipmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _shipmentDao.GetPendingAsync();
        return result.IsSuccess && result.Data != null;
    }
}

using FluentValidation;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Validators;

/// <summary>
/// Validator for AddPartToShipmentCommand.
/// </summary>
public class AddPartToShipmentCommandValidator : AbstractValidator<AddPartToShipmentCommand>
{
    public AddPartToShipmentCommandValidator()
    {
        RuleFor(x => x.PartNumber)
            .NotEmpty().WithMessage("Part number is required")
            .MaximumLength(50).WithMessage("Part number must not exceed 50 characters");

        RuleFor(x => x.ReceivedSkidCount)
            .GreaterThan(0).WithMessage("Received skid count must be greater than 0");

        // When HasDiscrepancy = true, ExpectedSkidCount and DiscrepancyNote are required
        When(x => x.HasDiscrepancy, () =>
        {
            RuleFor(x => x.ExpectedSkidCount)
                .NotNull().WithMessage("Expected skid count is required when there is a discrepancy")
                .GreaterThan(0).WithMessage("Expected skid count must be greater than 0");

            RuleFor(x => x.DiscrepancyNote)
                .NotEmpty().WithMessage("Discrepancy note is required when there is a discrepancy")
                .MaximumLength(500).WithMessage("Discrepancy note must not exceed 500 characters");
        });
    }
}

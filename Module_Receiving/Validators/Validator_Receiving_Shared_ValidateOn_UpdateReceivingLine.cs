using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators;

/// <summary>
/// Validator for UpdateReceivingLineCommand
/// Enforces business rules for line updates
/// </summary>
public class Validator_Receiving_Shared_ValidateOn_UpdateReceivingLine : AbstractValidator<CommandRequest_Receiving_Shared_Update_ReceivingLine>
{
    public Validator_Receiving_Shared_ValidateOn_UpdateReceivingLine()
    {
        RuleFor(x => x.LineId)
            .NotEmpty()
            .WithMessage("LineId is required");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty()
            .WithMessage("ModifiedBy is required");

        // Optional: Quantity validation (if provided)
        When(x => x.Quantity.HasValue, () =>
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero");
        });

        // Optional: Weight validation (if provided)
        When(x => x.Weight.HasValue, () =>
        {
            RuleFor(x => x.Weight)
                .GreaterThan(0)
                .WithMessage("Weight must be greater than zero");
        });

        // Optional: PackagesPerLoad validation (if provided)
        When(x => x.PackagesPerLoad.HasValue, () =>
        {
            RuleFor(x => x.PackagesPerLoad)
                .GreaterThan(0)
                .WithMessage("PackagesPerLoad must be greater than zero");
        });

        // Optional: WeightPerPackage validation (if provided)
        When(x => x.WeightPerPackage.HasValue, () =>
        {
            RuleFor(x => x.WeightPerPackage)
                .GreaterThan(0)
                .WithMessage("WeightPerPackage must be greater than zero");
        });
    }
}

using System;
using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators
{
    /// <summary>
    /// Validator for UpdateLoadDetailCommand.
    /// Validates load number and data field constraints.
    /// </summary>
    public class Validator_Receiving_Wizard_Data_UpdateLoadEntry : AbstractValidator<Command_ReceivingWizard_Data_UpdateLoadEntry>
    {
        public Validator_Receiving_Wizard_Data_UpdateLoadEntry()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage("Session ID is required");

            RuleFor(x => x.LoadNumber)
                .GreaterThan(0)
                .WithMessage("Load number must be greater than 0");

            RuleFor(x => x.WeightOrQuantity)
                .GreaterThan(0)
                .When(x => x.WeightOrQuantity.HasValue)
                .WithMessage("Weight or quantity must be greater than 0");

            RuleFor(x => x.HeatLot)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.HeatLot))
                .WithMessage("Heat lot cannot exceed 50 characters");

            RuleFor(x => x.PackageType)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.PackageType))
                .WithMessage("Package type cannot exceed 50 characters");

            RuleFor(x => x.PackagesPerLoad)
                .GreaterThan(0)
                .When(x => x.PackagesPerLoad.HasValue)
                .WithMessage("Packages per load must be greater than 0");
        }
    }
}

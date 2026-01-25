using System;
using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators
{
    /// <summary>
    /// Validator for CopyToLoadsCommand.
    /// Validates source load, target loads, and copy fields selection.
    /// </summary>
    public class Validator_Receiving_Wizard_Copy_FieldsToEmptyCells : AbstractValidator<Command_ReceivingWizard_Copy_FieldsToEmptyCells>
    {
        public Validator_Receiving_Wizard_Copy_FieldsToEmptyCells()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage("Session ID is required");

            RuleFor(x => x.SourceLoadNumber)
                .GreaterThan(0)
                .WithMessage("Source load number must be greater than 0");

            RuleFor(x => x.TargetLoadNumbers)
                .NotNull()
                .WithMessage("Target load numbers list cannot be null");

            RuleFor(x => x.FieldsToCopy)
                .IsInEnum()
                .WithMessage("Invalid copy fields selection");
        }
    }
}

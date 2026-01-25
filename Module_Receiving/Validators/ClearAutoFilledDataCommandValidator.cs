using System;
using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators
{
    /// <summary>
    /// Validator for ClearAutoFilledDataCommand.
    /// Validates target loads and fields to clear.
    /// </summary>
    public class ClearAutoFilledDataCommandValidator : AbstractValidator<Command_ReceivingWizard_Copy_ClearAutoFilledFields>
    {
        public ClearAutoFilledDataCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage("Session ID is required");

            RuleFor(x => x.TargetLoadNumbers)
                .NotNull()
                .WithMessage("Target load numbers list cannot be null");

            RuleFor(x => x.FieldsToClear)
                .IsInEnum()
                .WithMessage("Invalid fields to clear selection");
        }
    }
}

using System;
using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators
{
    /// <summary>
    /// Validator for UpdateStep1Command.
    /// Validates PO Number, Part ID, and Load Count.
    /// </summary>
    public class Validator_Receiving_Wizard_Data_EnterOrderAndPart : AbstractValidator<Command_ReceivingWizard_Data_EnterOrderAndPart>
    {
        public Validator_Receiving_Wizard_Data_EnterOrderAndPart()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage("Session ID is required");

            RuleFor(x => x.PONumber)
                .NotEmpty()
                .WithMessage("PO Number is required")
                .MaximumLength(50)
                .WithMessage("PO Number cannot exceed 50 characters")
                .Matches(@"^[A-Za-z0-9\-]+$")
                .WithMessage("PO Number must be alphanumeric with hyphens only");

            RuleFor(x => x.PartId)
                .GreaterThan(0)
                .WithMessage("Part ID must be a valid positive number");

            RuleFor(x => x.LoadCount)
                .InclusiveBetween(1, 100)
                .WithMessage("Load count must be between 1 and 100");
        }
    }
}

using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators;

/// <summary>
/// Validator for BulkCopyFieldsCommand
/// Ensures required fields are present for bulk copy operation
/// </summary>
public class Validator_Receiving_Wizard_ValidateOn_CopyFieldsToEmptyLoads : AbstractValidator<CommandRequest_Receiving_Wizard_Copy_FieldsToEmptyLoads>
{
    public Validator_Receiving_Wizard_ValidateOn_CopyFieldsToEmptyLoads()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("TransactionId is required");

        RuleFor(x => x.SourceLoadNumber)
            .GreaterThan(0)
            .WithMessage("SourceLoadNumber must be greater than zero");

        RuleFor(x => x.FieldsToCopy)
            .NotEmpty()
            .WithMessage("At least one field must be selected for copying");

        RuleFor(x => x.ModifiedBy)
            .NotEmpty()
            .WithMessage("ModifiedBy is required");
    }
}

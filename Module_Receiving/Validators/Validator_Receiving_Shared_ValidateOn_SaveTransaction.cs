using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators;

/// <summary>
/// Validator for CommandRequest_Receiving_Shared_Save_Transaction
/// Ensures all required fields are present and valid before saving
/// </summary>
public class Validator_Receiving_Shared_ValidateOn_SaveTransaction : AbstractValidator<CommandRequest_Receiving_Shared_Save_Transaction>
{
    public Validator_Receiving_Shared_ValidateOn_SaveTransaction()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("Session ID is required");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("Valid User ID is required");

        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("User Name is required")
            .MaximumLength(100)
            .WithMessage("User Name cannot exceed 100 characters");

        RuleFor(x => x.PONumber)
            .NotEmpty()
            .When(x => !x.IsNonPO)
            .WithMessage("PO Number is required for PO-based transactions")
            .Matches(@"^PO-\d{6}$")
            .When(x => !x.IsNonPO && !string.IsNullOrWhiteSpace(x.PONumber))
            .WithMessage("PO Number must be in format PO-XXXXXX (6 digits)");

        RuleFor(x => x.Loads)
            .NotEmpty()
            .WithMessage("At least one load is required")
            .Must(loads => loads.Count > 0)
            .WithMessage("Transaction must contain at least one load");

        RuleForEach(x => x.Loads).ChildRules(load =>
        {
            load.RuleFor(l => l.LoadNumber)
                .GreaterThan(0)
                .WithMessage("Load number must be greater than zero");

            load.RuleFor(l => l.PartId)
                .NotEmpty()
                .WithMessage("Part ID is required for each load");

            load.RuleFor(l => l.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero");

            load.RuleFor(l => l.PackagesPerLoad)
                .GreaterThan(0)
                .WithMessage("Packages per load must be greater than zero");
        });
    }
}

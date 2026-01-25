using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators;

/// <summary>
/// Validator for SaveWorkflowSessionCommand
/// Ensures session state is valid before persisting
/// </summary>
public class SaveWorkflowSessionCommandValidator : AbstractValidator<SaveWorkflowSessionCommand>
{
    public SaveWorkflowSessionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("Valid User ID is required");

        RuleFor(x => x.CurrentStep)
            .InclusiveBetween(1, 3)
            .WithMessage("Current step must be 1, 2, or 3");

        RuleFor(x => x.PONumber)
            .NotEmpty()
            .When(x => !x.IsNonPO)
            .WithMessage("PO Number is required when IsNonPO is false")
            .Matches(@"^PO-\d{6}$")
            .When(x => !x.IsNonPO && !string.IsNullOrWhiteSpace(x.PONumber))
            .WithMessage("PO Number must be in format PO-XXXXXX (6 digits)");

        RuleFor(x => x.LoadCount)
            .GreaterThan(0)
            .WithMessage("Load count must be greater than zero");

        RuleFor(x => x.PartId)
            .NotEmpty()
            .When(x => x.CurrentStep >= 1)
            .WithMessage("Part ID is required for Step 1 and beyond");
    }
}
